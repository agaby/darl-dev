// ***********************************************************************
// Assembly         : DarlCompiler
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="_ParserAction.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using Darl.ai;
using System;
using System.Collections.Generic;

namespace DarlCompiler.Parsing
{

    /// <summary>
    /// Class ParserAction.
    /// </summary>
    public abstract partial class ParserAction
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="ParserAction"/> class.
        /// </summary>
        public ParserAction() { }

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public virtual void Execute(ParsingContext context)
        {

        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return Resources.LabelActionUnknown; //should never happen
        }

    }

    /// <summary>
    /// Class ParserActionTable.
    /// </summary>
    [Serializable]
    public class ParserActionTable : Dictionary<BnfTerm, ParserAction> { }


}
