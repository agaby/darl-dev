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

    /// <summary>
    /// Class TokenEditorInfo.
    /// </summary>
    public class TokenEditorInfo
    {
        /// <summary>
        /// The type
        /// </summary>
        public readonly TokenType Type;
        /// <summary>
        /// The color
        /// </summary>
        public readonly TokenColor Color;
        /// <summary>
        /// The triggers
        /// </summary>
        public readonly TokenTriggers Triggers;
        /// <summary>
        /// The tool tip
        /// </summary>
        public string ToolTip;
        /// <summary>
        /// The underline type
        /// </summary>
        public int UnderlineType;
        /// <summary>
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

    /// <summary>
    /// Enum TokenColor
    /// </summary>
    public enum TokenColor
    {
        /// <summary>
        /// The text
        /// </summary>
        Text = 0,
        /// <summary>
        /// The keyword
        /// </summary>
        Keyword = 1,
        /// <summary>
        /// The comment
        /// </summary>
        Comment = 2,
        /// <summary>
        /// The identifier
        /// </summary>
        Identifier = 3,
        /// <summary>
        /// The string
        /// </summary>
        String = 4,
        /// <summary>
        /// The number
        /// </summary>
        Number = 5,
    }

    // (Comments are coming from visual studio integration package)
    //     Specifies a set of triggers that can be fired from an Microsoft.VisualStudio.Package.IScanner
    //     language parser.
    /// <summary>
    /// Enum TokenTriggers
    /// </summary>
    [Flags]
    public enum TokenTriggers
    {
        // Summary:
        //     Used when no triggers are set. This is the default.
        /// <summary>
        /// The none
        /// </summary>
        None = 0,
        //
        // Summary:
        //     A character that indicates that the start of a member selection has been
        //     parsed. In C#, this could be a period following a class name. In XML, this
        //     could be a < (the member select is a list of possible tags).
        /// <summary>
        /// The member select
        /// </summary>
        MemberSelect = 1,
        //
        // Summary:
        //     The opening or closing part of a language pair has been parsed. For example,
        //     in C#, a { or } has been parsed. In XML, a < or > has been parsed.
        /// <summary>
        /// The match braces
        /// </summary>
        MatchBraces = 2,
        //
        // Summary:
        //     A character that marks the start of a parameter list has been parsed. For
        //     example, in C#, this could be an open parenthesis, "(".
        /// <summary>
        /// The parameter start
        /// </summary>
        ParameterStart = 16,
        //
        // Summary:
        //     A character that separates parameters in a list has been parsed. For example,
        //     in C#, this could be a comma, ",".
        /// <summary>
        /// The parameter next
        /// </summary>
        ParameterNext = 32,
        //
        // Summary:
        //     A character that marks the end of a parameter list has been parsed. For example,
        //     in C#, this could be a close parenthesis, ")".
        /// <summary>
        /// The parameter end
        /// </summary>
        ParameterEnd = 64,
        //
        // Summary:
        //     A parameter in a method's parameter list has been parsed.
        /// <summary>
        /// The parameter
        /// </summary>
        Parameter = 128,
        //
        // Summary:
        //     This is a mask for the flags used to govern the IntelliSense Method Tip operation.
        //     This mask is used to isolate the values Microsoft.VisualStudio.Package.TokenTriggers.Parameter,
        //     Microsoft.VisualStudio.Package.TokenTriggers.ParameterStart, Microsoft.VisualStudio.Package.TokenTriggers.ParameterNext,
        //     and Microsoft.VisualStudio.Package.TokenTriggers.ParameterEnd.
        /// <summary>
        /// The method tip
        /// </summary>
        MethodTip = 240,
    }

    /// <summary>
    /// Enum TokenType
    /// </summary>
    public enum TokenType
    {
        /// <summary>
        /// The unknown
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// The text
        /// </summary>
        Text = 1,
        /// <summary>
        /// The keyword
        /// </summary>
        Keyword = 2,
        /// <summary>
        /// The identifier
        /// </summary>
        Identifier = 3,
        /// <summary>
        /// The string
        /// </summary>
        String = 4,
        /// <summary>
        /// The literal
        /// </summary>
        Literal = 5,
        /// <summary>
        /// The operator
        /// </summary>
        Operator = 6,
        /// <summary>
        /// The delimiter
        /// </summary>
        Delimiter = 7,
        /// <summary>
        /// The white space
        /// </summary>
        WhiteSpace = 8,
        /// <summary>
        /// The line comment
        /// </summary>
        LineComment = 9,
        /// <summary>
        /// The comment
        /// </summary>
        Comment = 10,
    }

}
