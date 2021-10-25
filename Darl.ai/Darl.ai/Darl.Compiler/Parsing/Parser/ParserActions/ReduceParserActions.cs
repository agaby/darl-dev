// ***********************************************************************
// Assembly         : DarlCompiler
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="ReduceParserActions.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************

using Darl.ai;

namespace DarlCompiler.Parsing
{

    /// <summary>
    /// Base class for more specific reduce actions.
    /// </summary>
    public partial class ReduceParserAction : ParserAction
    {
        /// <summary>
        /// The production
        /// </summary>
        public readonly Production Production;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReduceParserAction"/> class.
        /// </summary>
        /// <param name="production">The production.</param>
        public ReduceParserAction(Production production)
        {
            Production = production;
        }
        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return string.Format(Resources.LabelActionReduce, Production.ToStringQuoted());
        }

        /// <summary>
        /// Factory method for creating a proper type of reduce parser action.
        /// </summary>
        /// <param name="production">A Production to reduce.</param>
        /// <returns>Reduce action.</returns>
        public static ReduceParserAction Create(Production production)
        {
            var nonTerm = production.LValue;
            //List builder (non-empty production for list non-terminal) is a special case 
            var isList = nonTerm.Flags.IsSet(TermFlags.IsList);
            var isListBuilderProduction = isList && production.RValues.Count > 0 && production.RValues[0] == production.LValue;
            if (isListBuilderProduction)
                return new ReduceListBuilderParserAction(production);
            else if (nonTerm.Flags.IsSet(TermFlags.IsListContainer))
                return new ReduceListContainerParserAction(production);
            else if (nonTerm.Flags.IsSet(TermFlags.IsTransient))
                return new ReduceTransientParserAction(production);
            else
                return new ReduceParserAction(production);
        }

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public override void Execute(ParsingContext context)
        {
            var savedParserInput = context.CurrentParserInput;
            context.CurrentParserInput = GetResultNode(context);
            CompleteReduce(context);
            context.CurrentParserInput = savedParserInput;
        }

        /// <summary>
        /// Gets the result node.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>ParseTreeNode.</returns>
        protected virtual ParseTreeNode GetResultNode(ParsingContext context)
        {
            var childCount = Production.RValues.Count;
            int firstChildIndex = context.ParserStack.Count - childCount;
            var span = context.ComputeStackRangeSpan(childCount);
            var newNode = new ParseTreeNode(Production.LValue, span);
            for (int i = 0; i < childCount; i++)
            {
                var childNode = context.ParserStack[firstChildIndex + i];
                if (childNode.IsPunctuationOrEmptyTransient()) continue; //skip punctuation or empty transient nodes
                newNode.ChildNodes.Add(childNode);
            }//for i
            return newNode;
        }
        //Completes reduce: pops child nodes from the stack and pushes result node into the stack
        /// <summary>
        /// Completes the reduce.
        /// </summary>
        /// <param name="context">The context.</param>
        protected void CompleteReduce(ParsingContext context)
        {
            var resultNode = context.CurrentParserInput;
            var childCount = Production.RValues.Count;
            //Pop stack
            context.ParserStack.Pop(childCount);
            //Copy comment block from first child; if comments precede child node, they precede the parent as well. 
            if (resultNode.ChildNodes.Count > 0)
                resultNode.Comments = resultNode.ChildNodes[0].Comments;
            //Inherit precedence and associativity, to cover a standard case: BinOp->+|-|*|/; 
            // BinOp node should inherit precedence from underlying operator symbol. 
            if (Production.LValue.Flags.IsSet(TermFlags.InheritPrecedence))
                InheritPrecedence(resultNode);
            //Push new node into stack and move to new state
            //First read the state from top of the stack 
            context.CurrentParserState = context.ParserStack.Top.State;
            if (context.TracingEnabled)
                context.AddTrace(Resources.MsgTracePoppedState, Production.LValue.Name);
            #region comments on special case
            //Special case: if a non-terminal is Transient (ex: BinOp), then result node is not this NonTerminal, but its its child (ex: symbol). 
            // Shift action will invoke OnShifting on actual term being shifted (symbol); we need to invoke Shifting even on NonTerminal itself
            // - this would be more expected behavior in general. ImpliedPrecHint relies on this
            #endregion
            if (resultNode.Term != Production.LValue) //special case
                Production.LValue.OnShifting(context.SharedParsingEventArgs);
            // Shift to new state - execute shift over the non-terminal of the production. 
            var shift = context.CurrentParserState.Actions[Production.LValue];
            // Execute shift to new state
            shift.Execute(context);
            //Invoke Reduce event
            Production.LValue.OnReduced(context, Production, resultNode);
        }

        //This operation helps in situation when Bin expression is declared as BinExpr.Rule = expr + BinOp + expr; 
        // where BinOp is an OR-combination of operators. 
        // During parsing, when 'expr, BinOp, expr' is on the top of the stack, 
        // and incoming symbol is operator, we need to use precedence rule for deciding on the action. 
        /// <summary>
        /// Inherits the precedence.
        /// </summary>
        /// <param name="node">The node.</param>
        private void InheritPrecedence(ParseTreeNode node)
        {
            for (int i = 0; i < node.ChildNodes.Count; i++)
            {
                var child = node.ChildNodes[i];
                if (child.Precedence == Terminal.NoPrecedence) continue;
                node.Precedence = child.Precedence;
                node.Associativity = child.Associativity;
                return;
            }
        }

    }

    /// <summary>
    /// Reduces non-terminal marked as Transient by MarkTransient method.
    /// </summary>
    public class ReduceTransientParserAction : ReduceParserAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReduceTransientParserAction"/> class.
        /// </summary>
        /// <param name="production">The production.</param>
        public ReduceTransientParserAction(Production production) : base(production) { }

        /// <summary>
        /// Gets the result node.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>ParseTreeNode.</returns>
        protected override ParseTreeNode GetResultNode(ParsingContext context)
        {
            var topIndex = context.ParserStack.Count - 1;
            var childCount = Production.RValues.Count;
            for (int i = 0; i < childCount; i++)
            {
                var child = context.ParserStack[topIndex - i];
                if (child.IsPunctuationOrEmptyTransient()) continue;
                return child;
            }
            //Otherwise return an empty transient node; if it is part of the list, the list will skip it
            var span = context.ComputeStackRangeSpan(childCount);
            return new ParseTreeNode(Production.LValue, span);

        }
    }

    /// <summary>
    /// Reduces list created by MakePlusRule or MakeListRule methods.
    /// </summary>
    public class ReduceListBuilderParserAction : ReduceParserAction
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="ReduceListBuilderParserAction"/> class.
        /// </summary>
        /// <param name="production">The production.</param>
        public ReduceListBuilderParserAction(Production production) : base(production) { }

        /// <summary>
        /// Gets the result node.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>ParseTreeNode.</returns>
        protected override ParseTreeNode GetResultNode(ParsingContext context)
        {
            int childCount = Production.RValues.Count;
            int firstChildIndex = context.ParserStack.Count - childCount;
            var listNode = context.ParserStack[firstChildIndex]; //get the list already created - it is the first child node
            listNode.Span = context.ComputeStackRangeSpan(childCount);
            var listMember = context.ParserStack.Top; //next list member is the last child - at the top of the stack
            if (listMember.IsPunctuationOrEmptyTransient())
                return listNode;
            listNode.ChildNodes.Add(listMember);
            return listNode;
        }
    }

    //List container is an artificial non-terminal created by MakeStarRule method; the actual list is a direct child. 
    /// <summary>
    /// Class ReduceListContainerParserAction.
    /// </summary>
    public class ReduceListContainerParserAction : ReduceParserAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReduceListContainerParserAction"/> class.
        /// </summary>
        /// <param name="production">The production.</param>
        public ReduceListContainerParserAction(Production production) : base(production) { }

        /// <summary>
        /// Gets the result node.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>ParseTreeNode.</returns>
        protected override ParseTreeNode GetResultNode(ParsingContext context)
        {
            int childCount = Production.RValues.Count;
            int firstChildIndex = context.ParserStack.Count - childCount;
            var span = context.ComputeStackRangeSpan(childCount);
            var newNode = new ParseTreeNode(Production.LValue, span);
            if (childCount > 0)
            { //if it is not empty production - might happen for MakeStarRule
                var listNode = context.ParserStack[firstChildIndex]; //get the transient list with all members - it is the first child node
                newNode.ChildNodes.AddRange(listNode.ChildNodes);    //copy all list members
            }
            return newNode;

        }
    }

}
