/// <summary>
/// DaslGrammar.cs - Core module for the Darl.dev project.
/// </summary>

﻿// ***********************************************************************
// Assembly         : DaslLanguage
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-26-2015
// ***********************************************************************
// <copyright file="DaslGrammar.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using DarlCompiler.Parsing;
using DarlLanguage;


namespace DaslLanguage
{
    /// <summary>
    /// Extends the DARL grammar functionality
    /// </summary>
    [Language("Dasl", "1.0", "Dasl language, Copyright(c) Dr Andy's IP Ltd 2015")]
    public class DaslGrammar : DarlGrammar
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="DaslGrammar" /> class.
        /// </summary>
        public DaslGrammar() : base()
        {

        }

    }
}
