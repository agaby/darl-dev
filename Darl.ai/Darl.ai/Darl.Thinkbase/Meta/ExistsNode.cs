/// </summary>

﻿using Darl.Thinkbase;
using Darl.Thinkbase.Meta;
using DarlCompiler.Ast;
using DarlCompiler.Parsing;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Darl_standard.Darl.Thinkbase.Meta
{
    public class ExistsNode : UnaryDarlMetaNode
    {
        /// Initializes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="treeNode">The tree node.</param>
        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            try
            {
                base.Init(context, treeNode);
                var nodes = treeNode.GetMappedChildNodes();
                if (nodes.Any())
                    Argument = (DarlMetaNode)AddChild("-", nodes.Last());
            }
            catch
            {

            }
        }
        /// Does the evaluation.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <returns>
        /// The result of the evaluation
        /// </returns>
        protected override async Task<object> DoEvaluate(DarlCompiler.Interpreter.ScriptThread thread)
        {
            Prologue(thread);
            DarlResult? res = Argument != null ? (DarlResult)await Argument.Evaluate(thread) : null;
            var grammar = thread.Runtime.Language.Grammar as DarlMetaGrammar;
            if (grammar!.currentNode == null)
            {
                var res2 = new DarlResult(0, true);
                Epilogue(thread, res2);
                return res2;
            }
            if (res is null) //operate on object existence
            {
                if (grammar.currentNode.existence == null || !grammar.currentNode.existence.Any())
                    return new DarlResult(0, true);
                //return the truth of the statement: now and the objects existence overlap in time.
                var existence = new DarlResult("Existence", grammar.currentNode.existence, DarlResult.DataType.temporal);
                var nowNode = new NowNode();
                var now = await nowNode.Evaluate(thread) as DarlResult;
                var res2 = DarlResult.During(now, existence);
                Epilogue(thread, res2);
                return res2;
            }
            else //existence of an attribute
            {
                if (grammar.currentNode.properties == null)
                    return new DarlResult(0, true);
                var att = grammar.currentModel.FindAttributeExistence(grammar.currentNode.id, res.Value.ToString(), grammar.state);
                if (att == null)
                {
                    var res2 = new DarlResult(0, true);
                    Epilogue(thread, res2);
                    return res2;
                }
                var existence = new DarlResult("Existence", att, DarlResult.DataType.temporal);
                var nowNode = new NowNode();
                var now = await nowNode.Evaluate(thread) as DarlResult;
                thread.CurrentNode = Parent;
                var res3 =  DarlResult.During(now!, existence);
                Epilogue(thread, res3);
                return res3;
            }
        }


        /// Establishes dependencies and initializes constants
        /// </summary>
        /// <param name="dependencies">list of dependencies discovered</param>
        /// <param name="currentOutput">output for the rule being walked</param>
        /// <param name="context">The context.</param>
        public override void WalkDependencies(List<IntraSetDependency> dependencies, DarlMetaNode? currentOutput, ConstantContext context, IGraphModel model, GraphObject currentNode)
        {
            if (Argument != null)
                Argument.WalkDependencies(dependencies, currentOutput, context, model, currentNode);
        }

        /// Walks the saliences.
        /// </summary>
        /// <param name="saliency">The incoming saliency.</param>
        /// <param name="root">The map root.</param>
        /// <param name="currentOutput">The current output.</param>
        public override void WalkSaliences(double saliency, MetaRootNode root)
        {
            if (Argument != null)
                Argument.WalkSaliences(saliency, root);
        }

        /// Gets the preamble.
        /// </summary>
        /// <value>
        /// The preamble, used to reconstruct the source code.
        /// </value>
        public override string preamble
        {
            get
            {
                return "exists( ";
            }
        }
        public override string postamble
        {
            get
            {
                return ")";
            }
        }
    }
}
