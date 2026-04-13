/// <summary>
/// </summary>

﻿// ***********************************************************************
// Assembly         : DarlCompiler
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="_ISourceStream.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace DarlCompiler.Parsing
{

    /// <summary>
    /// Interface for Terminals to access the source stream and produce tokens.
    /// </summary>
    public interface ISourceStream
    {

        /// <summary>
        /// Returns the source text
        /// </summary>
        /// <value>The text.</value>
        string Text { get; }

        /// <summary>
        /// Gets or sets the start location (position, row, column) of the new token
        /// </summary>
        /// <value>The location.</value>
        SourceLocation Location { get; set; }

        /// <summary>
        /// Gets or sets the current position in the source file. When reading the value, returns Location.Position value.
        /// When a new value is assigned, the Location is modified accordingly.
        /// </summary>
        /// <value>The position.</value>
        int Position { get; set; }

        /// <summary>
        /// Gets or sets the current preview position in the source file. Must be greater or equal to Location.Position
        /// </summary>
        /// <value>The preview position.</value>
        int PreviewPosition { get; set; }
        /// <summary>
        /// Gets a char at preview position
        /// </summary>
        /// <value>The preview character.</value>
        char PreviewChar { get; }
        /// <summary>
        /// Gets the char at position next after the PrevewPosition
        /// </summary>
        /// <value>The next preview character.</value>
        char NextPreviewChar { get; }    //char at PreviewPosition+1

        /// <summary>
        /// Creates a new token based on current preview position.
        /// </summary>
        /// <param name="terminal">A terminal associated with the token.</param>
        /// <returns>New token.</returns>
        Token CreateToken(Terminal terminal);

        /// <summary>
        /// Creates a new token based on current preview position and sets its Value field.
        /// </summary>
        /// <param name="terminal">A terminal associated with the token.</param>
        /// <param name="value">The value associated with the token.</param>
        /// <returns>New token.</returns>
        Token CreateToken(Terminal terminal, object value);

        /// <summary>
        /// Tries to match the symbol with the text at current preview position.
        /// </summary>
        /// <param name="symbol">A symbol to match</param>
        /// <returns>True if there is a match; otherwise, false.</returns>
        bool MatchSymbol(string symbol);

        /// <summary>
        /// EOFs this instance.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        bool EOF();

        /*
        //This member is intentionally removed from ISourceStream and made private in SourceStream class. The purpose is to discourage
         its use or imitation - it produces a new string object which means new garbage for GC. All Darl-defined Terminal classes 
         are implemented without it, but you can always reproduce the implementation in your custom code if you really need it
        string GetPreviewText();
         */

    }//interface


}
