/// </summary>

﻿// ***********************************************************************
// Assembly         : DarlCompiler
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="TokenEditorInfo.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;

namespace DarlCompiler.Parsing
{
    // Helper classes for information used by syntax highlighters and editors
    // TokenColor, TokenTriggers and TokenType are copied from the Visual studio integration assemblies. 
    //  Each terminal/token would have its TokenEditorInfo that can be used either by VS integration package 
    //   or any editor for syntax highlighting.

    /// Class TokenEditorInfo.
    /// </summary>
    public class TokenEditorInfo
    {
        /// The type
        /// </summary>
        public readonly TokenType Type;
        /// The color
        /// </summary>
        public readonly TokenColor Color;
        /// The triggers
        /// </summary>
        public readonly TokenTriggers Triggers;
        /// The tool tip
        /// </summary>
        public string ToolTip;
        /// The underline type
        /// </summary>
        public int UnderlineType;
        /// Initializes a new instance of the <see cref="TokenEditorInfo"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="color">The color.</param>
        /// <param name="triggers">The triggers.</param>
        public TokenEditorInfo(TokenType type, TokenColor color, TokenTriggers triggers)
        {
            Type = type;
            Color = color;
            Triggers = triggers;
        }

    }

    /// Enum TokenColor
    /// </summary>
    public enum TokenColor
    {
        /// The text
        /// </summary>
        Text = 0,
        /// The keyword
        /// </summary>
        Keyword = 1,
        /// The comment
        /// </summary>
        Comment = 2,
        /// The identifier
        /// </summary>
        Identifier = 3,
        /// The string
        /// </summary>
        String = 4,
        /// The number
        /// </summary>
        Number = 5,
    }

    // (Comments are coming from visual studio integration package)
    //     Specifies a set of triggers that can be fired from an Microsoft.VisualStudio.Package.IScanner
    //     language parser.
    /// Enum TokenTriggers
    /// </summary>
    [Flags]
    public enum TokenTriggers
    {
        // Summary:
        //     Used when no triggers are set. This is the default.
        /// The none
        /// </summary>
        None = 0,
        //
        // Summary:
        //     A character that indicates that the start of a member selection has been
        //     parsed. In C#, this could be a period following a class name. In XML, this
        //     could be a < (the member select is a list of possible tags).
        /// The member select
        /// </summary>
        MemberSelect = 1,
        //
        // Summary:
        //     The opening or closing part of a language pair has been parsed. For example,
        //     in C#, a { or } has been parsed. In XML, a < or > has been parsed.
        /// The match braces
        /// </summary>
        MatchBraces = 2,
        //
        // Summary:
        //     A character that marks the start of a parameter list has been parsed. For
        //     example, in C#, this could be an open parenthesis, "(".
        /// The parameter start
        /// </summary>
        ParameterStart = 16,
        //
        // Summary:
        //     A character that separates parameters in a list has been parsed. For example,
        //     in C#, this could be a comma, ",".
        /// The parameter next
        /// </summary>
        ParameterNext = 32,
        //
        // Summary:
        //     A character that marks the end of a parameter list has been parsed. For example,
        //     in C#, this could be a close parenthesis, ")".
        /// The parameter end
        /// </summary>
        ParameterEnd = 64,
        //
        // Summary:
        //     A parameter in a method's parameter list has been parsed.
        /// The parameter
        /// </summary>
        Parameter = 128,
        //
        // Summary:
        //     This is a mask for the flags used to govern the IntelliSense Method Tip operation.
        //     This mask is used to isolate the values Microsoft.VisualStudio.Package.TokenTriggers.Parameter,
        //     Microsoft.VisualStudio.Package.TokenTriggers.ParameterStart, Microsoft.VisualStudio.Package.TokenTriggers.ParameterNext,
        //     and Microsoft.VisualStudio.Package.TokenTriggers.ParameterEnd.
        /// The method tip
        /// </summary>
        MethodTip = 240,
    }

    /// Enum TokenType
    /// </summary>
    public enum TokenType
    {
        /// The unknown
        /// </summary>
        Unknown = 0,
        /// The text
        /// </summary>
        Text = 1,
        /// The keyword
        /// </summary>
        Keyword = 2,
        /// The identifier
        /// </summary>
        Identifier = 3,
        /// The string
        /// </summary>
        String = 4,
        /// The literal
        /// </summary>
        Literal = 5,
        /// The operator
        /// </summary>
        Operator = 6,
        /// The delimiter
        /// </summary>
        Delimiter = 7,
        /// The white space
        /// </summary>
        WhiteSpace = 8,
        /// The line comment
        /// </summary>
        LineComment = 9,
        /// The comment
        /// </summary>
        Comment = 10,
    }

}
