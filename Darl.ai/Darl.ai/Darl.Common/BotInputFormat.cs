/// </summary>

﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DarlCommon
{
    public class BotInputFormat
    {
        /// Gets or sets the categories.
        /// </summary>
        /// <value>The categories.</value>
        [Display(Name = "Categories defined", Description = "All the categories found in the rule set for this input")]
        public List<string>? Categories { get; set; }

        /// Gets or sets the categories.
        /// </summary>
        /// <value>The categories.</value>
        [Display(Name = "Sets defined", Description = "All the sets for this input")]
        public List<SetDefinition>? Sets { get; set; }


        /// Gets or sets the increment.
        /// </summary>
        /// <value>The increment.</value>
        [Display(Name = "Edit increment", Description = "optional increment for numeric spinners where supported")]
        public double Increment { get; set; }

        /// Gets or sets the type of the input.
        /// </summary>
        /// <value>The type of the input.</value>
        [Display(Name = "Type", Description = "The data type of the input")]
        [Required]
        public InputFormat.InputType InType { get; set; }

        /// Gets or sets the maximum length.
        /// </summary>
        /// <value>The maximum length.</value>
        [Display(Name = "Maximum length", Description = "The maximum length if textual, (0 means no limit)")]
        public int MaxLength { get; set; }

        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        [Display(Name = "The name", Description = "The name of the input defined in the rule set")]
        [Required]
        public string Name { get; set; } = string.Empty;

        /// Gets or sets the numeric maximum.
        /// </summary>
        /// <value>The numeric maximum.</value>)
        [Display(Name = "Maximum value", Description = "The maximum value for a numeric input (can be null)")]
        public double NumericMax { get; set; }

        /// Gets or sets the numeric minimum.
        /// </summary>
        /// <value>The numeric minimum.</value>
        [Display(Name = "Minimum value", Description = "The maximum value for a numeric input (can be null)")]
        public double NumericMin { get; set; }

        /// Gets or sets the regular expression.
        /// </summary>
        /// <value>The regular expression.</value>
        [Display(Name = "Regular Expression", Description = "A regular expression generating a validation error for a textual input if not met")]
        public string? Regex { get; set; }

        /// Gets or sets a value indicating whether to show sets.
        /// </summary>
        /// <value><c>true</c> if true show sets; otherwise, <c>false</c>.</value>
        [Display(Name = "Show sets", Description = "If true, a numeric input is displayed like a categorical one, using set names as categories.")]
        public bool ShowSets { get; set; }

        /// Gets or sets a value indicating whether to allow the user to give a fuzzy value.
        /// </summary>
        /// <value><c>true</c> if true no fuzziness; otherwise, <c>false</c> permits fuzzy values.</value>
        [Display(Name = "Enforce crisp", Description = "If true, then only a singleton value for a numeric input, or a single category should be selectable in the UI.")]
        public bool EnforceCrisp { get; set; } = false;

        /// The input type
        /// </summary>
        public enum InputType
        {
            /// Numerical input
            /// </summary>
            numeric = 0,
            /// categorical input
            /// </summary>
            categorical = 1,
            /// Textual input
            /// </summary>
            textual = 2,
        }
    }
}
