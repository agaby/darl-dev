/// <summary>
/// AzureCredentials.cs - Core module for the Darl.dev project.
/// </summary>

﻿using System.ComponentModel.DataAnnotations;

namespace DarlCommon
{
    public class AzureCredentials
    {
        [Display(Name = "Azure storage API Key", Description = "Supplied by Microsoft Azure")]
        [Required]
        public string? AzureAPIKey { get; set; }
    }
}
