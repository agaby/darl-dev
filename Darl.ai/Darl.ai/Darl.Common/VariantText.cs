/// </summary>

﻿// ***********************************************************************
// Assembly         : CS.AutomationTest.Web
// Author           : Andrew
// Created          : 11-04-2014
//
// Last Modified By : Andrew
// Last Modified On : 02-27-2015
// ***********************************************************************
// <copyright file="VariantText.cs" company="Dr Andy's IP Ltd (BVI)">
//     Copyright ©  2014
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.ComponentModel.DataAnnotations;

namespace DarlCommon
{

    /// Class VariantText.
    /// </summary>
    [Serializable]
    public class VariantText
    {
        /// Gets or sets the language.
        /// </summary>
        /// <value>The language.</value>
        [Display(Name = "The language", Description = "The language for this text")]
        [Required]
        public string? Language { get; set; }
        /// Gets or sets the text.
        /// </summary>
        /// <value>The text.</value>
        [Display(Name = "The text", Description = "The text for this language")]
        [Required]
        public string? Text { get; set; }
    }
}
