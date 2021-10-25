// ***********************************************************************
// Assembly         : DarlCompiler
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="ParsingEventArgs.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;

namespace DarlCompiler.Parsing
{
    /// <summary>
    /// Class ParsingEventArgs.
    /// </summary>
    public class ParsingEventArgs : EventArgs
    {
        /// <summary>
        /// The context
        /// </summary>
        public readonly ParsingContext Context;
        /// <summary>
        /// Initializes a new instance of the <see cref="ParsingEventArgs"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public ParsingEventArgs(ParsingContext context)
        {
            Context = context;
        }
    }

    /// <summary>
    /// Class ReducedEventArgs.
    /// </summary>
    public class ReducedEventArgs : ParsingEventArgs
    {
        /// <summary>
        /// The reduced production
        /// </summary>
        public readonly Production ReducedProduction;
        /// <summary>
        /// The result node
        /// </summary>
        public readonly ParseTreeNode ResultNode;
        /// <summary>
        /// Initializes a new instance of the <see cref="ReducedEventArgs"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="reducedProduction">The reduced production.</param>
        /// <param name="resultNode">The result node.</param>
        public ReducedEventArgs(ParsingContext context, Production reducedProduction, ParseTreeNode resultNode)
            : base(context)
        {
            ReducedProduction = reducedProduction;
            ResultNode = resultNode;
        }
    }

    /// <summary>
    /// Class ValidateTokenEventArgs.
    /// </summary>
    public class ValidateTokenEventArgs : ParsingEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidateTokenEventArgs"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public ValidateTokenEventArgs(ParsingContext context) : base(context) { }

        /// <summary>
        /// Gets the token.
        /// </summary>
        /// <value>The token.</value>
        public Token Token
        {
            get { return Context.CurrentToken; }
        }//Token

        /// <summary>
        /// Replaces the token.
        /// </summary>
        /// <param name="token">The token.</param>
        public void ReplaceToken(Token token)
        {
            Context.CurrentToken = token;
        }
        /// <summary>
        /// Sets the error.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="messageArgs">The message arguments.</param>
        public void SetError(string errorMessage, params object[] messageArgs)
        {
            Context.CurrentToken = Context.CreateErrorToken(errorMessage, messageArgs);
        }
        //Rejects the token; use it when there's more than one terminal that can be used to scan the input and ValidateToken is used
        // to help Scanner make the decision. Once the token is rejected, the scanner will move to the next Terminal (with lower priority)
        // and will try to produce token. 
        /// <summary>
        /// Rejects the token.
        /// </summary>
        public void RejectToken()
        {
            Context.CurrentToken = null;
        }
    }

}
