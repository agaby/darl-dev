/// <summary>
/// DaslState.cs - Core module for the Darl.dev project.
/// </summary>

﻿// ***********************************************************************
// Assembly         : DarlInfAPI
// Author           : Andrew
// Created          : 05-14-2015
//
// Last Modified By : Andrew
// Last Modified On : 05-14-2015
// ***********************************************************************
// <copyright file="DarlState.cs" company="Dr Andy's IP Ltd (BVI)">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DarlCommon
{
    /// <summary>
    /// Class DaslState.
    /// </summary>
    /// <remarks>A time stamped state of a system, reconstructible from the associated values</remarks>
    public class DaslState
    {
        /// <summary>
        /// Gets or sets the time stamp.
        /// </summary>
        /// <value>The time stamp.</value>
        [Required]
        [Display(Name = "The time stamp", Description = "The moment these values changed or became valid")]
        public DateTime timeStamp { get; set; }

        /// <summary>
        /// Gets or sets the values.
        /// </summary>
        /// <value>The values.</value>
        [Required]
        [Display(Name = "The values", Description = "A set of values that changed or became valid at the given time")]
        public List<DarlVar> values { get; set; } = new List<DarlVar>();
    }
}