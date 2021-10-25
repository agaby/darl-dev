// ***********************************************************************
// Assembly         : CS.AutomationTest.Web
// Author           : Andrew
// Created          : 11-04-2014
//
// Last Modified By : Andrew
// Last Modified On : 02-27-2015
// ***********************************************************************
// <copyright file="LanguageText.cs" company="Dr Andy's IP Ltd (BVI)">
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
    /// Class LanguageText.
    /// </summary>
    [Serializable]
    public class LanguageText
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        [Display(Name = "The name", Description = "The name of the text item as used in the rule set")]
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        /// <value>The text.</value>
        [Display(Name = "The text", Description = "The text to display for the default language")]
        [Required]
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the variant list.
        /// </summary>
        /// <value>The variant list.</value>
        [Display(Name = "variant text", Description = "The text to display for other languages")]
        [Required]
        public List<VariantText> VariantList { get; set; }

    }
}
