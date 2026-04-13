/// </summary>

﻿// ***********************************************************************
// Assembly         : DarlCompiler
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="LanguageAttribute.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;

namespace DarlCompiler.Parsing
{

    /// Class LanguageAttribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class LanguageAttribute : Attribute
    {
        /// Initializes a new instance of the <see cref="LanguageAttribute"/> class.
        /// </summary>
        public LanguageAttribute() : this(null) { }
        /// Initializes a new instance of the <see cref="LanguageAttribute"/> class.
        /// </summary>
        /// <param name="languageName">Name of the language.</param>
        public LanguageAttribute(string languageName) : this(languageName, "1.0", string.Empty) { }

        /// Initializes a new instance of the <see cref="LanguageAttribute"/> class.
        /// </summary>
        /// <param name="languageName">Name of the language.</param>
        /// <param name="version">The version.</param>
        /// <param name="description">The description.</param>
        public LanguageAttribute(string languageName, string version, string description)
        {
            _languageName = languageName;
            _version = version;
            _description = description;
        }

        /// Gets the name of the language.
        /// </summary>
        /// <value>The name of the language.</value>
        public string LanguageName
        {
            get { return _languageName; }
            /// The _language name
            /// </summary>
        }

        readonly string _languageName;

        /// Gets the version.
        /// </summary>
        /// <value>The version.</value>
        public string Version
        {
            get { return _version; }
            /// The _version
            /// </summary>
        }

        readonly string _version;

        /// Gets the description.
        /// </summary>
        /// <value>The description.</value>
        public string Description
        {
            get { return _description; }
            /// The _description
            /// </summary>
        }

        readonly string _description;

        /// Gets the value.
        /// </summary>
        /// <param name="grammarClass">The grammar class.</param>
        /// <returns>LanguageAttribute.</returns>
        public static LanguageAttribute GetValue(Type grammarClass)
        {
            object[] attrs = grammarClass.GetCustomAttributes(typeof(LanguageAttribute), true);
            if (attrs != null && attrs.Length > 0)
            {
                LanguageAttribute la = attrs[0] as LanguageAttribute;
                return la;
            }
            return null;
        }

    }
}
