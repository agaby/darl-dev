// ***********************************************************************
// Assembly         : CS.AutomationTest.Web
// Author           : Andrew
// Created          : 11-04-2014
//
// Last Modified By : Andrew
// Last Modified On : 02-27-2015
// ***********************************************************************
// <copyright file="InputFormat.cs" company="Dr Andy's IP Ltd (BVI)">
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
    /// The format of a Darl input
    /// </summary>
    [Serializable]
    public class InputFormat
    {
        /// <summary>
        /// Gets or sets the categories.
        /// </summary>
        /// <value>The categories.</value>
        [Display(Name = "Categories defined", Description = "All the categories found in the rule set for this input")]
        [ReadOnly(true)]
        public List<string>? Categories { get; set; }

        /// <summary>
        /// Gets or sets the increment.
        /// </summary>
        /// <value>The increment.</value>
        [Display(Name = "Edit increment", Description = "optional increment for numeric spinners where supported")]
        public double Increment { get; set; }

        /// <summary>
        /// Gets or sets the type of the input.
        /// </summary>
        /// <value>The type of the input.</value>
        [Display(Name = "Type", Description = "The data type of the input")]
        [ReadOnly(true)]
        [Required]
        public InputFormat.InputType InType { get; set; }

        /// <summary>
        /// Gets or sets the maximum length.
        /// </summary>
        /// <value>The maximum length.</value>
        [Display(Name = "Maximum length", Description = "The maximum length if textual, (0 means no limit)")]
        public int MaxLength { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        [Display(Name = "The name", Description = "The name of the input defined in the rule set")]
        [ReadOnly(true)]
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the numeric maximum.
        /// </summary>
        /// <value>The numeric maximum.</value>)
        [Display(Name = "Maximum value", Description = "The maximum value for a numeric input (can be null)")]
        public double NumericMax { get; set; }

        /// <summary>
        /// Gets or sets the numeric minimum.
        /// </summary>
        /// <value>The numeric minimum.</value>
        [Display(Name = "Minimum value", Description = "The minimum value for a numeric input (can be null)")]
        public double NumericMin { get; set; }

        /// <summary>
        /// Gets or sets the regular expression.
        /// </summary>
        /// <value>The regular expression.</value>
        [Display(Name = "Regular Expression", Description = "A regular expression generating a validation error for a textual input if not met")]
        public string Regex { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show sets.
        /// </summary>
        /// <value><c>true</c> if true show sets; otherwise, <c>false</c>.</value>
        [Display(Name = "Show sets", Description = "If true, a numeric input is displayed like a categorical one, using set names as categories.")]
        public bool ShowSets { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to allow the user to give a fuzzy value.
        /// </summary>
        /// <value><c>true</c> if true no fuzziness; otherwise, <c>false</c> permits fuzzy values.</value>
        [Display(Name = "Enforce crisp", Description = "If true, then only a singleton value for a numeric input, or a single category should be selectable in the UI.")]
        public bool EnforceCrisp { get; set; } = false;

        [Display(Name = "Path to the input", Description = "If the source data is json, this is a jsonpath expression to locate the data, if XML, Xpath, or a lineage to match variables by conceptual type. ")]
        public string path { get; set; }

        /// <summary>
        /// The input type
        /// </summary>
        public enum InputType
        {
            /// <summary>
            /// Numerical input
            /// </summary>
            numeric = 0,
            /// <summary>
            /// categorical input
            /// </summary>
            categorical = 1,
            /// <summary>
            /// Textual input
            /// </summary>
            textual = 2,
            /// <summary>
            /// Temporal input
            /// </summary>
            temporal = 3
        }
    }
}
