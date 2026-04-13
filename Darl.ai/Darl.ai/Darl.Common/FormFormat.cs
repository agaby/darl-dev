/// <summary>
/// FormFormat.cs - Core module for the Darl.dev project.
/// </summary>

﻿// ***********************************************************************
// Assembly         : CS.AutomationTest.Web
// Author           : Andrew
// Created          : 11-04-2014
//
// Last Modified By : Andrew
// Last Modified On : 02-27-2015
// ***********************************************************************
// <copyright file="FormFormat.cs" company="Dr Andy's IP Ltd (BVI)">
//     Copyright ©  2014
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;


namespace DarlCommon
{
    /// <summary>
    /// Contains formatting information for a Darl Form
    /// </summary>
    [Serializable]
    public class FormFormat
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="FormFormat" /> class.
        /// </summary>
        public FormFormat()
        {
            DefaultQuestions = 1;
            Edited = false;
            InputFormatList = new List<InputFormat>();
            OutputFormatList = new List<OutputFormat>();
        }

        /// <summary>
        /// Gets or sets the default questions.
        /// </summary>
        /// <value>The default questions.</value>

        [Display(Name = "Default questions", Description = "Maximum number of questions to display per pass")]
        [Range(1, 10)]
        [Required]
        public int DefaultQuestions { get; set; } = 1;

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="FormFormat"/> is edited.
        /// </summary>
        /// <value><c>true</c> if edited; otherwise, <c>false</c>.</value>
        [ReadOnly(true)]
        public bool Edited { get; set; }
        /// <summary>
        /// Gets or sets the input format list.
        /// </summary>
        /// <value>The input format list.</value>
        [Display(Name = "Inputs", Description = "Formatting for all the inputs in the rule set")]
        [Required]
        public List<InputFormat> InputFormatList { get; set; }
        /// <summary>
        /// Gets or sets the output format list.
        /// </summary>
        /// <value>The output format list.</value>
        [Display(Name = "Outputs", Description = "Formatting for all the outputs in the rule set")]
        [Required]
        public List<OutputFormat> OutputFormatList { get; set; }
    }
}
