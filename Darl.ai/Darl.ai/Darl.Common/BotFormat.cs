/// <summary>
/// BotFormat.cs - Core module for the Darl.dev project.
/// </summary>

﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DarlCommon
{
    public class BotFormat
    {
        /// <summary>
        /// Gets or sets the input format list.
        /// </summary>
        /// <value>The input format list.</value>
        [Display(Name = "Inputs", Description = "Formatting for all the inputs in the rule set")]
        [Required]
        public List<BotInputFormat>? InputFormatList { get; set; }
        /// <summary>
        /// Gets or sets the output format list.
        /// </summary>
        /// <value>The output format list.</value>
        [Display(Name = "Outputs", Description = "Formatting for all the outputs in the rule set")]
        [Required]
        public List<BotOutputFormat>? OutputFormatList { get; set; }

        /// <summary>
        /// Gets or sets the list of store names.
        /// </summary>
        /// <value>The store list.</value>
        [Display(Name = "Stores", Description = "Names of the stores in the rule set")]
        public List<string> Stores { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the list of strings.
        /// </summary>
        /// <value>The String list.</value>
        [Display(Name = "Strings", Description = "String constants in the rule set")]
        public Dictionary<string, string> Strings { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the list of constants.
        /// </summary>
        /// <value>The Constants list.</value>
        [Display(Name = "Constants", Description = "Numeric constants in the rule set")]
        public Dictionary<string, double> Constants { get; set; } = new Dictionary<string, double>();

        /// <summary>
        /// Gets or sets the list of sequences.
        /// </summary>
        /// <value>The Sequence list.</value>
        [Display(Name = "Sequences", Description = "Sequence constants in the rule set")]
        public Dictionary<string, List<List<string>>> Sequences { get; set; } = new Dictionary<string, List<List<string>>>();
    }
}
