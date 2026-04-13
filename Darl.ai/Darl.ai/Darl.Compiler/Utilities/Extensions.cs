/// <summary>
/// Extensions.cs - Core module for the Darl.dev project.
/// </summary>

﻿// ***********************************************************************
// Assembly         : DarlCompiler
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="Extensions.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace DarlCompiler.Parsing
{
    /// <summary>
    /// Class ParsingEnumExtensions.
    /// </summary>
    public static class ParsingEnumExtensions
    {

        /// <summary>
        /// Determines whether the specified flag is set.
        /// </summary>
        /// <param name="flags">The flags.</param>
        /// <param name="flag">The flag.</param>
        /// <returns><c>true</c> if the specified flag is set; otherwise, <c>false</c>.</returns>
        public static bool IsSet(this TermFlags flags, TermFlags flag)
        {
            return (flags & flag) != 0;
        }
        /// <summary>
        /// Determines whether the specified flag is set.
        /// </summary>
        /// <param name="flags">The flags.</param>
        /// <param name="flag">The flag.</param>
        /// <returns><c>true</c> if the specified flag is set; otherwise, <c>false</c>.</returns>
        public static bool IsSet(this LanguageFlags flags, LanguageFlags flag)
        {
            return (flags & flag) != 0;
        }
        /// <summary>
        /// Determines whether the specified option is set.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="option">The option.</param>
        /// <returns><c>true</c> if the specified option is set; otherwise, <c>false</c>.</returns>
        public static bool IsSet(this ParseOptions options, ParseOptions option)
        {
            return (options & option) != 0;
        }
        /// <summary>
        /// Determines whether the specified option is set.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="option">The option.</param>
        /// <returns><c>true</c> if the specified option is set; otherwise, <c>false</c>.</returns>
        public static bool IsSet(this TermListOptions options, TermListOptions option)
        {
            return (options & option) != 0;
        }
        /// <summary>
        /// Determines whether the specified flag is set.
        /// </summary>
        /// <param name="flags">The flags.</param>
        /// <param name="flag">The flag.</param>
        /// <returns><c>true</c> if the specified flag is set; otherwise, <c>false</c>.</returns>
        public static bool IsSet(this ProductionFlags flags, ProductionFlags flag)
        {
            return (flags & flag) != 0;
        }
    }

}
