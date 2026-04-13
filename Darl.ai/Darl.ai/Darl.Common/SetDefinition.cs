/// <summary>
/// </summary>

﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DarlCommon
{
    /// <summary>
    /// Set definitions for formats.
    /// </summary>
    public class SetDefinition
    {
        /// <summary>
        /// Name of the set
        /// </summary>
        [Display(Name = "Name of the set", Description = "The name used in the rules")]
        public string name { get; set; }
        /// <summary>
        /// The set values
        /// </summary>
        [Display(Name = "The set values", Description = "An ascending set of values defining a convex set.")]
        public List<double> values { get; set; } = new List<double>();
    }
}
