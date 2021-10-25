// ***********************************************************************
// Assembly         : DarlCompiler
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="PrecedenceBasedParserAction.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using Darl.ai;

namespace DarlCompiler.Parsing
{

    /// <summary>
    /// Class PrecedenceBasedParserAction.
    /// </summary>
    public class PrecedenceBasedParserAction : ConditionalParserAction
    {
        /// <summary>
        /// The _shift action
        /// </summary>
        readonly ShiftParserAction _shiftAction;

        /// <summary>
        /// The _reduce action
        /// </summary>
        readonly ReduceParserAction _reduceAction;

        /// <summary>
        /// Initializes a new instance of the <see cref="PrecedenceBasedParserAction"/> class.
        /// </summary>
        /// <param name="shiftTerm">The shift term.</param>
        /// <param name="newShiftState">New state of the shift.</param>
        /// <param name="reduceProduction">The reduce production.</param>
        public PrecedenceBasedParserAction(BnfTerm shiftTerm, ParserState newShiftState, Production reduceProduction)
        {
            _reduceAction = new ReduceParserAction(reduceProduction);
            var reduceEntry = new ConditionalEntry(CheckMustReduce, _reduceAction, "(Precedence comparison)");
            base.ConditionalEntries.Add(reduceEntry);
            base.DefaultAction = _shiftAction = new ShiftParserAction(shiftTerm, newShiftState);
        }

        /// <summary>
        /// Checks the must reduce.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private bool CheckMustReduce(ParsingContext context)
        {
            var input = context.CurrentParserInput;
            var stackCount = context.ParserStack.Count;
            var prodLength = _reduceAction.Production.RValues.Count;
            for (int i = 1; i <= prodLength; i++)
            {
                var prevNode = context.ParserStack[stackCount - i];
                if (prevNode == null) continue;
                if (prevNode.Precedence == BnfTerm.NoPrecedence) continue;
                //if previous operator has the same precedence then use associativity
                if (prevNode.Precedence == input.Precedence)
                    return (input.Associativity == Associativity.Left); //if true then Reduce
                else
                    return (prevNode.Precedence > input.Precedence); //if true then Reduce
            }
            //If no operators found on the stack, do shift
            return false;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return string.Format(Resources.LabelActionOp, _shiftAction.NewState.Name, _reduceAction.Production.ToStringQuoted());
        }

    }


}
