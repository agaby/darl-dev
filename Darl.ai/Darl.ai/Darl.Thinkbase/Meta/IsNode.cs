using DarlCompiler.Ast;
using DarlCompiler.Parsing;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Darl.Thinkbase.Meta
{
    public class IsNode : DarlMetaNode
    {

        /// <summary>
        /// The left side
        /// </summary>
        DarlMetaNode Left;//left side of "is" is always an identifier of an input or output or a store
        /// <summary>
        /// The right side
        /// </summary>
        DarlMetaNode Right;

        /// <summary>
        /// Initializes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="treeNode">The tree node.</param>
        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            var nodes = treeNode.GetMappedChildNodes();
            Left = (DarlMetaNode)AddChild("-", nodes[0]);
            Right = (DarlMetaNode)AddChild("-", nodes[1]);
        }

        /// <summary>
        /// Establishes dependencies and initializes constants
        /// </summary>
        /// <param name="dependencies">list of dependencies discovered</param>
        /// <param name="currentOutput">output for the rule being walked</param>
        /// <param name="context">The context.</param>
        public override void WalkDependencies(List<IntraSetDependency> dependencies, DarlMetaNode? currentOutput, ConstantContext context, IGraphModel model, GraphObject currentNode)
        {
            Left.WalkDependencies(dependencies, currentOutput, context, model, currentNode);
            Right.WalkDependencies(dependencies, currentOutput, context, model, currentNode);
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
            Prologue(thread);
            var res1 = (DarlResult)await Left.Evaluate(thread);
            //handle certainty based responses
            if (res1.IsUnknown())
            {
                if (Right is AbsentNode)
                {
                    var res = new DarlResult(1.0, false);
                    Epilogue(thread, res);
                    return res;
                }
                else
                {
                    var res = new DarlResult(0.0, true); //changed 05/05/2015, was false but prevented salience calcs
                    Epilogue(thread, res);
                    return res;
                }
            }
            else if (Right is AbsentNode)
            {
                var res = new DarlResult(1.0 - res1.GetWeight(), false);
                Epilogue(thread, res);
                return res;
            }
            else if (Right is PresentNode)
            {
                var res =  new DarlResult(res1.GetWeight(), false);
                Epilogue(thread, res);
                return res;
            }
            DarlResult result = new DarlResult(0.0, true);
            if (Left is DarlMetaIdentifierNode)
            {
                switch (((DarlMetaIdentifierNode)Left).identType)
                {
                    case DarlMetaIdentifierNode.IdentifierType.numeric_input:
                    case DarlMetaIdentifierNode.IdentifierType.numeric_output:
                    case DarlMetaIdentifierNode.IdentifierType.temporal_input:
                    case DarlMetaIdentifierNode.IdentifierType.temporal_output:
                    case DarlMetaIdentifierNode.IdentifierType.arity_input:
                        //rhs is set or comparison
                        if (Right is DarlMetaIdentifierNode) //must be set
                        {
                            DarlResult res2 = (DarlResult)await Right.Evaluate(thread);
                            DarlResult res3 = res2.Equal(res1);
                            var res = new DarlResult((double)res3.values[0], false);
                            Epilogue(thread, res);
                            return res;
                        }
                        else // must be comparative
                        {
                            thread.CurrentScope.Parameters = new object[1];
                            thread.CurrentScope.SetParameter(0, res1);
                            var res = (DarlResult)await Right.Evaluate(thread);
                            Epilogue(thread, res);
                            return res;
                        }

                    case DarlMetaIdentifierNode.IdentifierType.categorical_input:
                    case DarlMetaIdentifierNode.IdentifierType.categorical_output:
                        {
                            DarlResult res2 = (DarlResult)await Right.Evaluate(thread);
                            var res3 = res2.Equal(res1);
                            if (res3.values.Count == 0)
                            {
                                var res =  new DarlResult(res3.GetWeight(), false);
                                Epilogue(thread, res);
                                return res;
                            }
                            Epilogue(thread, res3);
                            return res3;
                        }
                    case DarlMetaIdentifierNode.IdentifierType.presence_input:
                        break;
                    case DarlMetaIdentifierNode.IdentifierType.textual_output:
                    case DarlMetaIdentifierNode.IdentifierType.textual_input:
                        {
                            thread.CurrentScope.Parameters = new object[1];
                            thread.CurrentScope.SetParameter(0, res1);
                            DarlResult res = (DarlResult)await Right.Evaluate(thread);
                            Epilogue(thread, res);
                            return res;
                        }
                }
            }
            else //StoreNode
            {
                thread.CurrentScope.Parameters = new object[1];
                thread.CurrentScope.SetParameter(0, res1);
                var res = (DarlResult)await Right.Evaluate(thread);
                Epilogue(thread, res);
                return res;
            }
            Epilogue(thread, result);
            return result;
        }
        /// <summary>
        /// Gets the midamble.
        /// </summary>
        /// <value>
        /// The midamble, used to reconstruct the source code.
        /// </value>
        public override string midamble
        {
            get
            {
                return " is ";
            }
        }

        /// <summary>
        /// Walks the saliences.
        /// </summary>
        /// <param name="saliency">The incoming saliency.</param>
        /// <param name="root">The map root.</param>
        /// <param name="currentOutput">The current output.</param>
        public override void WalkSaliences(double saliency, MetaRootNode root)
        {
            Left.WalkSaliences(saliency, root);
            Right.WalkSaliences(saliency, root);
        }

    }
}