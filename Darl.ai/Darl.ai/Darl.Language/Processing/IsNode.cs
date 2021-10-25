using System.Collections.Generic;
using DarlCompiler.Ast;
using DarlCompiler.Parsing;
using System.Threading.Tasks;

namespace DarlLanguage.Processing
{
    /// <summary>
    /// Implements the "is" function, which returns the degree of truth of the statement surrounding it.
    /// </summary>
    public class IsNode : DarlNode
    {
        /// <summary>
        /// The left side
        /// </summary>
        DarlNode Left;//left side of "is" is always an identifier of an input or output or a store
        /// <summary>
        /// The right side
        /// </summary>
        DarlNode Right;

        /// <summary>
        /// Initializes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="treeNode">The tree node.</param>
        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            var nodes = treeNode.GetMappedChildNodes();
            Left = (DarlNode)AddChild("-", nodes[0]);
            Right = (DarlNode)AddChild("-", nodes[1]);
        }

        /// <summary>
        /// Does the evaluation.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <returns>
        /// The result of the evaluation
        /// </returns>
        protected async override Task<object> DoEvaluate(DarlCompiler.Interpreter.ScriptThread thread)
        {
            thread.CurrentNode = this;  //standard prologue
            var res1 = (DarlResult) await Left.Evaluate(thread);
            //handle certainty based responses
            if (res1.IsUnknown())
            {
                if (Right is AbsentNode)
                    return new DarlResult(1.0, false);
                if (Right is PresentNode)
                    return new DarlResult(0.0, true); //changed 05/05/2015, was false but prevented salience calcs
                return new DarlResult(0.0, true);  
            }
            else if (Right is AbsentNode)
            {
                return new DarlResult(1.0 - res1.GetWeight(), false);
            }
            else if (Right is PresentNode)
            {
                return new DarlResult(res1.GetWeight(), false);
            }
            DarlResult result = new DarlResult(0.0, true);
            if (Left is DarlIdentifierNode)
            {
                switch (((DarlIdentifierNode)Left).identType)
                {
                    case DarlIdentifierNode.IdentifierType.numeric_input:
                    case DarlIdentifierNode.IdentifierType.numeric_output:
                    case DarlIdentifierNode.IdentifierType.temporal_input:
                    case DarlIdentifierNode.IdentifierType.temporal_output:
                    case DarlIdentifierNode.IdentifierType.arity_input:
                        //rhs is set or comparison
                        if (Right is DarlIdentifierNode) //must be set
                        {
                            DarlResult res2 = (DarlResult) await Right.Evaluate(thread);
                            DarlResult res3 = res2.Equal(res1);
                            return new DarlResult((double)res3.values[0], false);
                        }
                        else // must be comparative
                        {
                            thread.CurrentScope.Parameters = new object[1];
                            thread.CurrentScope.SetParameter(0, res1);
                            return (DarlResult) await Right.Evaluate(thread);
                        }

                    case DarlIdentifierNode.IdentifierType.categorical_input:
                    case DarlIdentifierNode.IdentifierType.categorical_output:
                        {
                            DarlResult res2 = (DarlResult) await Right.Evaluate(thread);
                            var res3 = res2.Equal(res1);
                            if (res3.values.Count == 0)
                            {
                                return new DarlResult(res3.GetWeight(), false);
                            }
                            return res3;
                        }
                    case DarlIdentifierNode.IdentifierType.presence_input:
                        break;
                    case DarlIdentifierNode.IdentifierType.textual_output:
                    case DarlIdentifierNode.IdentifierType.textual_input:
                        {
                            thread.CurrentScope.Parameters = new object[1];
                            thread.CurrentScope.SetParameter(0, res1);
                            return await Right.Evaluate(thread);
                        }
                }
            }
            else //StoreNode
            {
                thread.CurrentScope.Parameters = new object[1];
                thread.CurrentScope.SetParameter(0, res1);
                return await Right.Evaluate(thread);
            }

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
            Left.WalkDependencies(dependencies, currentOutput, context);
            Right.WalkDependencies(dependencies, currentOutput, context);
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
                return "is ";
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
            Left.WalkSaliences(saliency, root, currentRuleSet, currentOutput);
            Right.WalkSaliences(saliency, root, currentRuleSet, currentOutput);
        }
    }
}
