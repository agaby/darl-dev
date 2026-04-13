// ***********************************************************************
// Assembly         : DarlCompiler
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="BnfExpression.cs" company="Dr Andy's IP LLC">
//     Copyright   2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;

namespace DarlCompiler.Parsing
{

    //BNF expressions are represented as OR-list of Plus-lists of BNF terms
    /// Class BnfExpressionData.
    /// </summary>
    internal class BnfExpressionData : List<BnfTermList>
    {
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            try
            {
                string[] pipeArr = new string[this.Count];
                for (int i = 0; i < this.Count; i++)
                {
                    BnfTermList seq = this[i];
                    string[] seqArr = new string[seq.Count];
                    for (int j = 0; j < seq.Count; j++)
                        seqArr[j] = seq[j].ToString();
                    pipeArr[i] = String.Join("+", seqArr);
                }
                return String.Join("|", pipeArr);
            }
            catch (Exception e)
            {
                return "(error: " + e.Message + ")";
            }

        }
    }

    /// Class BnfExpression.
    /// </summary>
    public class BnfExpression : BnfTerm
    {

        /// Initializes a new instance of the <see cref="BnfExpression"/> class.
        /// </summary>
        /// <param name="element">The element.</param>
        public BnfExpression(BnfTerm element)
            : this()
        {
            Data[0].Add(element);
        }
        /// Initializes a new instance of the <see cref="BnfExpression"/> class.
        /// </summary>
        public BnfExpression()
            : base(null)
        {
            Data = new BnfExpressionData();
            Data.Add(new BnfTermList());
        }

        /// The data
        /// </summary>
        internal BnfExpressionData Data;
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return Data.ToString();
        }

        #region Implicit cast operators
        /// Performs an implicit conversion from <see cref="System.String"/> to <see cref="BnfExpression"/>.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator BnfExpression(string symbol)
        {
            return new BnfExpression(Grammar.CurrentGrammar.ToTerm(symbol));
        }
        //It seems better to define one method instead of the following two, with parameter of type BnfTerm -
        // but that's not possible - it would be a conversion from base type of BnfExpression itself, which
        // is not allowed in c#
        /// Performs an implicit conversion from <see cref="Terminal"/> to <see cref="BnfExpression"/>.
        /// </summary>
        /// <param name="term">The term.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator BnfExpression(Terminal term)
        {
            return new BnfExpression(term);
        }
        /// Performs an implicit conversion from <see cref="NonTerminal"/> to <see cref="BnfExpression"/>.
        /// </summary>
        /// <param name="nonTerminal">The non terminal.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator BnfExpression(NonTerminal nonTerminal)
        {
            return new BnfExpression(nonTerminal);
        }
        #endregion


    }

}
