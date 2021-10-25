// ***********************************************************************
// Assembly         : CS.AutomationTest.Web
// Author           : Andrew
// Created          : 11-04-2014
//
// Last Modified By : Andrew
// Last Modified On : 02-27-2015
// ***********************************************************************
// <copyright file="OutputFormat.cs" company="Dr Andy's IP Ltd (BVI)">
//     Copyright ©  2014
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DarlCommon
{
    /// <summary>
    /// Defines the format of a Darl output
    /// </summary>
    [Serializable]
    public class OutputFormat
    {
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="OutputFormat" /> is hidden.
        /// </summary>
        /// <value><c>true</c> if hide; otherwise, <c>false</c>.</value>
        [Display(Name = "Hide", Description = "If true this output is not presented to the user at form completion")]
        [Required]
        public bool Hide { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        [Display(Name = "Name", Description = "The name of the output in the Darl rule set")]
        [ReadOnly(true)]
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the type of the output.
        /// </summary>
        /// <value>The type of the output.</value>
        [Display(Name = "Type", Description = "The data type of the output")]
        [ReadOnly(true)]
        [Required]
        public OutputFormat.OutType OutputType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use a [score bar].
        /// </summary>
        /// <value><c>true</c> if [score bar]; otherwise, <c>false</c>.</value>
        [Display(Name = "Display type", Description = "The preferred display method")]
        [Required]
        public DisplayType displayType { get; set; }

        /// <summary>
        /// Gets or sets the color of the score bar.
        /// </summary>
        /// <value>The color of the score bar.</value>
        [Display(Name = "Bar color", Description = "The color to use if a score bar is selected in CSS format")]
        public string ScoreBarColor { get; set; }

        /// <summary>
        /// Gets or sets the score bar maximum value.
        /// </summary>
        /// <value>The score bar maximum value.</value>
        [Display(Name = "Bar max val", Description = "The maximum value if a score bar is selected")]
        public double ScoreBarMaxVal { get; set; }

        /// <summary>
        /// Gets or sets the score bar minimum value.
        /// </summary>
        /// <value>The score bar minimum value.</value>
        [Display(Name = "Bar min val", Description = "The minimum value if a score bar is selected")]
        public double ScoreBarMinVal { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="OutputFormat" /> is uncertainty.
        /// </summary>
        /// <value><c>true</c> if uncertainty; otherwise, <c>false</c>.</value>
        [Display(Name = "Uncertainty", Description = "Determines if uncertainty data is displayed for this output")]
        public bool Uncertainty { get; set; }

        /// <summary>
        /// Gets or sets the value format.
        /// </summary>
        /// <value>The value format.</value>
        [Display(Name = "Value format", Description = "Format for the precision etc in standard 'C' form")]
        public string ValueFormat { get; set; }

        [Display(Name = "Path to the output", Description = "If the source data is json, this is a jsonpath expression to locate the data or place the result, if XML, Xpath, or a lineage to match variables by conceptual type. ")]
        public string path { get; set; }


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
            textual = 2,
            /// <summary>
            /// Temporal output
            /// </summary>
            temporal = 3
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
            /// As a scorebar
            /// </summary>
            ScoreBar = 2,
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
