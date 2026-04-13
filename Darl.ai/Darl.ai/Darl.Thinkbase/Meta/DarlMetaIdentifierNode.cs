/// </summary>

﻿using DarlCompiler.Ast;
using DarlCompiler.Interpreter;
using DarlCompiler.Parsing;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Darl.Thinkbase.Meta
{
    public class DarlMetaIdentifierNode : DarlMetaNode
    {
        /// The type of the identifier
        /// </summary>
        public enum IdentifierType
        {
            /// The categorical_input
            /// </summary>
            categorical_input,
            /// The numeric_input
            /// </summary>
            numeric_input,
            /// The arity_input
            /// </summary>
            arity_input,
            /// The presence_input
            /// </summary>
            presence_input,
            /// The textual_input
            /// </summary>
            textual_input,
            /// The textual_output
            /// </summary>
            textual_output,
            /// The categorical_output
            /// </summary>
            categorical_output,
            /// The numeric_output
            /// </summary>
            numeric_output,
            /// The category
            /// </summary>
            category,
            /// The set
            /// </summary>
            set,
            /// The string_constant
            /// </summary>
            string_constant,
            /// The numeric_constant
            /// </summary>
            numeric_constant,
            /// a sequence constant
            /// </summary>
            sequence_constant,
            /// The rule_identifier
            /// </summary>
            rule_identifier,
            /// The map_input
            /// </summary>
            map_input,
            /// The map_output
            /// </summary>
            map_output,
            /// The store name.
            /// </summary>
            store_io,
            /// Temporal input
            /// </summary>
            temporal_input,
            /// temporal output
            /// </summary>
            temporal_output,
            /// temporal constant
            /// </summary>
            temporal_constant,
            /// dynamic categorical input
            /// </summary>
            dynamic_categorical_input,
            /// Network output
            /// </summary>
            network_output,
            duration_constant,
            lineage_constant
        };

        /// Gets the identifier's name.
        /// </summary>
        /// <value>
        /// The identifier's name.
        /// </value>
        public string name { get; private set; }

        /// Gets the type of the identifier.
        /// </summary>
        /// <value>
        /// The type of the identifier.
        /// </value>
        public IdentifierType identType { get; private set; }

        private Binding _accessor;

        private DarlResult fixedResult { get; set; }

        /// Update linked values when updated.
        /// </summary>
        public List<DarlMetaIdentifierNode> links = new List<DarlMetaIdentifierNode>();

        /// Initializes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="treeNode">The tree node.</param>
        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            name = (string)treeNode.Token.Value;
            identType = (IdentifierType)Enum.Parse(typeof(IdentifierType), treeNode.Term.Name, true);
        }

        /// Establishes dependencies and initializes constants
        /// </summary>
        /// <param name="dependencies">list of dependencies discovered</param>
        /// <param name="currentOutput">output for the rule being walked</param>
        /// <param name="context">The context.</param>
        public override void WalkDependencies(List<IntraSetDependency> dependencies, DarlMetaNode? currentOutput, ConstantContext context, IGraphModel model, GraphObject currentNode)
        {
            if (IsConstant())
            {
                switch (identType)
                {
                    case IdentifierType.category:
                        fixedResult = new DarlResult("", name);
                        break;
                    case IdentifierType.set:
                        //look up value
                        if (context.inputs.ContainsKey(context.controllingIO))
                        {
                            fixedResult = context.inputs[context.controllingIO].sets[name];
                        }
                        else
                        {
                            fixedResult = context.outputs[context.controllingIO].sets[name];
                        }
                        break;
                    case IdentifierType.numeric_constant:
                        //look up value
                        fixedResult = new DarlResult(context.constants[name].Value);
                        break;
                    case IdentifierType.string_constant:
                        fixedResult = new DarlResult("", context.strings[name].Value, DarlResult.DataType.textual);
                        break;
                    case IdentifierType.duration_constant:
                        fixedResult = new DarlResult("", context.durations[name].Value, DarlResult.DataType.duration);
                        break;
                    case IdentifierType.temporal_constant:
                        fixedResult = new DarlResult("", context.durations[name].Value, DarlResult.DataType.temporal);
                        break;
                    case IdentifierType.lineage_constant:
                        if (context.lineages.ContainsKey(name))
                        {
                            fixedResult = new DarlResult("", context.lineages[name].Value, DarlResult.DataType.textual);
                        }
                        else
                        {
                            if (context.parseContext != null)
                            {
                                context.parseContext.AddMessage(DarlCompiler.ErrorLevel.Error, this.ErrorAnchor, $"Lineage constant {name} not defined.");
                            }
                        }
                        break;
                }
            }
            else
            {
                if (identType == IdentifierType.numeric_output || identType == IdentifierType.categorical_output || identType == IdentifierType.textual_output || identType == IdentifierType.temporal_output || identType == IdentifierType.network_output)
                {//we only care about o-o dependencies
                    dependencies.Add(new IntraSetDependency { outputAsInput = this.name, output = currentOutput.GetName() });
                }
                context.controllingIO = this.name;
            }
        }

        public override bool IsConstant()
        {
            switch (identType)
            {
                case IdentifierType.category:
                case IdentifierType.set:
                case IdentifierType.string_constant:
                case IdentifierType.numeric_constant:
                case IdentifierType.sequence_constant:
                case IdentifierType.temporal_constant:
                case IdentifierType.duration_constant:
                case IdentifierType.lineage_constant:
                    return true;
                default:
                    return false;
            }
        }

        public override string GetName()
        {
            return name;
        }

        public override string preamble
        {
            get
            {
                switch (identType)
                {
                    case IdentifierType.store_io:
                        return $"{name}[";
                    default:
                        return $"{name}";
                }
            }
        }

        protected override async Task<object> DoEvaluate(DarlCompiler.Interpreter.ScriptThread thread)
        {
            Prologue(thread);
            if (IsConstant())
            {
                Epilogue(thread, fixedResult);
                return fixedResult;
            }
            _accessor = thread.Bind(name, BindingRequestFlags.Read);
            this.Evaluate = _accessor.GetValueRef; // Optimization - directly set method ref to accessor's method. EvaluateReader;
            var result = (DarlResult) await this.Evaluate(thread);
            Epilogue(thread, result);
            return result;
        }

        public override void WalkSaliences(double saliency, MetaRootNode root)
        {
            if (identType == IdentifierType.categorical_input || identType == IdentifierType.numeric_input || identType == IdentifierType.textual_input || identType == IdentifierType.temporal_input || identType == IdentifierType.dynamic_categorical_input)
            {
                if (root.inputs.ContainsKey(name))
                    root.inputs[name].WalkSaliences(saliency, root);
            }
        }
    }
}