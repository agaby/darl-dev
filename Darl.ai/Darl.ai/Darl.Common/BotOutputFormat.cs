/// </summary>

﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DarlCommon
{
    public class BotOutputFormat
    {

        /// Gets or sets the categories.
        /// </summary>
        /// <value>The categories.</value>
        [Display(Name = "Categories defined", Description = "All the categories for this input")]
        public List<string>? Categories { get; set; }

        /// Gets or sets the categories.
        /// </summary>
        /// <value>The categories.</value>
        [Display(Name = "Sets defined", Description = "All the sets for this input")]
        public List<SetDefinition>? Sets { get; set; }


        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        [Display(Name = "Name", Description = "The name of the output in the Darl rule set")]
        [Required]
        public string Name { get; set; } = string.Empty;

        /// Gets or sets the type of the output.
        /// </summary>
        /// <value>The type of the output.</value>
        [Display(Name = "Type", Description = "The data type of the output")]
        [Required]
        public OutputFormat.OutType OutputType { get; set; }

        /// Gets or sets a value indicating whether to use a [score bar].
        /// </summary>
        /// <value><c>true</c> if [score bar]; otherwise, <c>false</c>.</value>
        [Display(Name = "Function type", Description = "The function performed")]
        [Required]
        public DisplayType displayType { get; set; }

        /// Gets or sets the value format.
        /// </summary>
        /// <value>The value format.</value>
        [Display(Name = "Value format", Description = "Format for the precision etc in standard 'C' form")]
        public string? ValueFormat { get; set; }


        /// The possible output types
        /// </summary>
        public enum OutType
        {
            /// numeric
            /// </summary>
            numeric = 0,
            /// categorical
            /// </summary>
            categorical = 1,
            /// Textual output
            /// </summary>
            textual = 2
        }
        /// The way in which the output is displayed.
        /// </summary>
        public enum DisplayType
        {
            /// As text
            /// </summary>
            Text = 1,
            /// As a link
            /// </summary>
            Link = 3,
            /// As a redirect to another named form
            /// </summary>
            Redirect = 4
        }
    }
}
