// ***********************************************************************
// Assembly         : CS.AutomationTest.Web
// Author           : Andrew
// Created          : 11-04-2014
//
// Last Modified By : Andrew
// Last Modified On : 02-27-2015
// ***********************************************************************
// <copyright file="LanguageFormat.cs" company="Dr Andy's IP Ltd (BVI)">
//     Copyright ©  2014
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DarlCommon
{
    /// <summary>
    /// Language format
    /// </summary>
    [Serializable]
    public class LanguageFormat
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LanguageFormat" /> class.
        /// </summary>
        public LanguageFormat()
        {
            LanguageList = new List<LanguageText>();
            DefaultLanguage = "en";
        }
        /// <summary>
        /// Gets or sets the language list.
        /// </summary>
        /// <value>The language list.</value>
        [Display(Name = "Text items for the form", Description = "All the text items used can be edited in this list")]
        [Required]
        public List<LanguageText> LanguageList { get; set; }

        /// <summary>
        /// Gets or sets the default language.
        /// </summary>
        /// <value>The default language.</value>
        [Display(Name = "The default language", Description = "Two letter ISO standard language")]
        [Required]
        public string DefaultLanguage { get; set; }
    }


}
