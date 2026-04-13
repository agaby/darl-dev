/// <summary>
/// DarlIdentifierNode.cs - Core module for the Darl.dev project.
/// </summary>

﻿using DarlCompiler.Ast;
using DarlCompiler.Interpreter;
using DarlCompiler.Parsing;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DarlLanguage.Processing
{
    /// <summary>
    /// Implements an identifier
    /// </summary>
    public class DarlIdentifierNode : DarlNode
    {
        /// <summary>
        /// The rule identifier key used to locate the current rule set for namespace resolution.
        /// </summary>
        public static string ruleIdentKey = "CurrentRuleIdentifier";

        /// <summary>
        /// The type of the identifier
        /// </summary>
        public enum IdentifierType
        {
            /// <summary>
            /// The categorical_input
            /// </summary>
            categorical_input,
            /// <summary>
            /// The numeric_input
            /// </summary>
            numeric_input,
            /// <summary>
            /// The arity_input
            /// </summary>
            arity_input,
            /// <summary>
            /// The presence_input
            /// </summary>
            presence_input,
            /// <summary>
            /// The textual_input
            /// </summary>
            textual_input,
            /// <summary>
            /// The textual_output
            /// </summary>
            textual_output,
            /// <summary>
            /// The categorical_output
            /// </summary>
            categorical_output,
            /// <summary>
            /// The numeric_output
            /// </summary>
            numeric_output,
            /// <summary>
            /// The category
            /// </summary>
            category,
            /// <summary>
            /// The set
            /// </summary>
            set,
            /// <summary>
            /// The string_constant
            /// </summary>
            string_constant,
            /// <summary>
            /// The numeric_constant
            /// </summary>
            numeric_constant,
            /// <summary>
            /// a sequence constant
            /// </summary>
            sequence_constant,
            /// <summary>
            /// The rule_identifier
            /// </summary>
            rule_identifier,
            /// <summary>
            /// The map_input
            /// </summary>
            map_input,
            /// <summary>
            /// The map_output
            /// </summary>
            map_output,
            /// <summary>
            /// The store name.
            /// </summary>
            store_io,
            /// <summary>
            /// Temporal input
            /// </summary>
            temporal_input,
            /// <summary>
            /// temporal output
            /// </summary>
            temporal_output,
            /// <summary>
            /// temporal constant
            /// </summary>
            temporal_constant,
            ///<summary>
            /// dynamic categorical input
            /// </summary>
            dynamic_categorical_input,
        };

        /// <summary>
        /// Gets the identifier's name.
        /// </summary>
        /// <value>
        /// The identifier's name.
        /// </value>
        public string name { get; private set; }

        /// <summary>
        /// Gets the ruleset name.
        /// </summary>
        /// <value>
        /// The ruleset name.
        /// </value>
        public string ruleset { get; private set; }

        /// <summary>
        /// Gets the type of the identifier.
        /// </summary>
        /// <value>
        /// The type of the identifier.
        /// </value>
        public IdentifierType identType { get; private set; }

        private Binding _accessor;

        private DarlResult fixedResult { get; set; }

        /// <summary>
        /// Update linked values when updated.
        /// </summary>
        public List<DarlIdentifierNode> links = new List<DarlIdentifierNode>();

        /// <summary>
        /// Initializes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="treeNode">The tree node.</param>
        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            name = (string)treeNode.Token.Value;
            identType = (IdentifierType)Enum.Parse(typeof(IdentifierType), treeNode.Term.Name, true);
            switch (identType)
            {
                case IdentifierType.rule_identifier:
                    context.CurrentNamespace = name;
                    break;
                case IdentifierType.arity_input:
                case IdentifierType.categorical_input:
                case IdentifierType.categorical_output:
                case IdentifierType.numeric_input:
                case IdentifierType.numeric_output:
                case IdentifierType.textual_input:
                case IdentifierType.textual_output:
                case IdentifierType.temporal_input:
                case IdentifierType.temporal_output:
                case IdentifierType.store_io:
                case IdentifierType.dynamic_categorical_input:
                    ruleset = context.CurrentNamespace;
                    break;
                default:
                    ruleset = string.Empty;
                    break;

            }
        }

        /// <summary>
        /// Does the evaluation.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <returns>
        /// The result of the evaluation
        /// </returns>
        protected override async Task<object> DoEvaluate(DarlCompiler.Interpreter.ScriptThread thread)
        {
            if (IsConstant())
            {
                return fixedResult;
            }
            thread.CurrentNode = this;  //standard prologue
            _accessor = thread.Bind(ruleset + "." + name, BindingRequestFlags.Read);
            this.Evaluate = _accessor.GetValueRef; // Optimization - directly set method ref to accessor's method. EvaluateReader;
            var result = await this.Evaluate(thread);
            thread.CurrentNode = Parent; //standard epilogue
            return result;
        }

        /// <summary>
        /// Establishes dependencies and initializes constants
        /// </summary>
        /// <param name="dependencies">list of dependencies discovered</param>
        /// <param name="currentOutput">output for the rule being walked</param>
        /// <param name="context">The context.</param>
        public override void WalkDependencies(List<IntraSetDependency> dependencies, DarlNode currentOutput, ConstantContext context)
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
                    case IdentifierType.sequence_constant:
                        //look up value
                        {
                            fixedResult = new DarlResult("", context.sequences[name].Value, DarlResult.DataType.sequence);
                        }
                        break;
                    case IdentifierType.numeric_constant:
                        //look up value
                        fixedResult = new DarlResult(context.constants[name].Value);
                        break;
                    case IdentifierType.string_constant:
                        fixedResult = new DarlResult("", context.strings[name].Value, DarlResult.DataType.textual);
                        break;
                    case IdentifierType.temporal_constant:
                        fixedResult = new DarlResult("", context.durations[name].Value, DarlResult.DataType.duration);
                        break;
                }
            }
            else
            {
                if (identType == IdentifierType.numeric_output || identType == IdentifierType.categorical_output || identType == IdentifierType.textual_output || identType == IdentifierType.temporal_output)
                {//we only care about o-o dependencies
                    dependencies.Add(new IntraSetDependency { outputAsInput = this.name, output = currentOutput.GetName() });
                }
                context.controllingIO = this.name;
            }
        }


        /// <summary>
        /// Walks the saliences.
        /// </summary>
        /// <param name="saliency">The incoming saliency.</param>
        /// <param name="root">The map root.</param>
        /// <param name="currentRuleSet">The current rule set.</param>
        /// <param name="currentOutput">The current output.</param>
        public override void WalkSaliences(double saliency, MapRootNode root, string currentRuleSet, string currentOutput)
        {
            if (identType == IdentifierType.categorical_input || identType == IdentifierType.numeric_input || identType == IdentifierType.textual_input || identType == IdentifierType.temporal_input || identType == IdentifierType.dynamic_categorical_input)
            {
                var wire = root.NavigateDest(name, currentRuleSet);
                if (wire != null)//might be unconnected input
                    wire.WalkSaliences(saliency * 2.0, root, currentRuleSet, currentOutput); //experimental scaling to favor deeper inputs
            }
            //Add output handling
            if (identType == IdentifierType.numeric_output || identType == IdentifierType.categorical_output || identType == IdentifierType.textual_output || identType == IdentifierType.temporal_output)
            {
                var wire = root.NavigateDest(name, currentRuleSet);
                if (wire != null)//might be unconnected output
                    wire.WalkSaliences(saliency, root, currentRuleSet, currentOutput);
                else //could be internal ruleset dependence.
                {
                    var o = root.NavigateInternal(name, currentRuleSet);
                    if (o != null)
                    {
                        o.WalkSaliences(saliency * 2.0, root, currentRuleSet, name);//experimental scaling to favor deeper inputs
                    }
                }
            }
        }

        /// <summary>
        /// Determines whether this instance is constant.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is constant; otherwise, <c>false</c>.
        /// </returns>
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
                    case IdentifierType.rule_identifier:
                        if (Parent is CompIoNode)
                            return $"{name}.";
                        return "";
                    case IdentifierType.store_io:
                        return $"{name}[";
                    default:
                        return $"{name} ";
                }
            }
        }

    }
}
