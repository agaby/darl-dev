using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DarlCommon
{
    public class BotOutputFormat
    {

        /// <summary>
        /// Gets or sets the categories.
        /// </summary>
        /// <value>The categories.</value>
        [Display(Name = "Categories defined", Description = "All the categories for this input")]
        public List<string>? Categories { get; set; }

        /// <summary>
        /// Gets or sets the categories.
        /// </summary>
        /// <value>The categories.</value>
        [Display(Name = "Sets defined", Description = "All the sets for this input")]
        public List<SetDefinition>? Sets { get; set; }


        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        [Display(Name = "Name", Description = "The name of the output in the Darl rule set")]
        [Required]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the type of the output.
        /// </summary>
        /// <value>The type of the output.</value>
        [Display(Name = "Type", Description = "The data type of the output")]
        [Required]
        public OutputFormat.OutType OutputType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use a [score bar].
        /// </summary>
        /// <value><c>true</c> if [score bar]; otherwise, <c>false</c>.</value>
        [Display(Name = "Function type", Description = "The function performed")]
        [Required]
        public DisplayType displayType { get; set; }

        /// <summary>
        /// Gets or sets the value format.
        /// </summary>
        /// <value>The value format.</value>
        [Display(Name = "Value format", Description = "Format for the precision etc in standard 'C' form")]
        public string? ValueFormat { get; set; }


        /// <summary>
        /// The possible output types
        /// </summary>
        public enum OutType
        {
            /// <summary>
            /// numeric
            /// </summary>
            numeric = 0,
            /// <summary>
            /// categorical
            /// </summary>
            categorical = 1,
            /// <summary>
            /// Textual output
            /// </summary>
            textual = 2
        }
        /// <summary>
        /// The way in which the output is displayed.
        /// </summary>
        public enum DisplayType
        {
            /// <summary>
            /// As text
            /// </summary>
            Text = 1,
            /// <summary>
            /// As a link
            /// </summary>
            Link = 3,
            /// <summary>
            /// As a redirect to another named form
            /// </summary>
            Redirect = 4
        }
    }
}
