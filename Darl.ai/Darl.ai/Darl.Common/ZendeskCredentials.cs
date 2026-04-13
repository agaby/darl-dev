/// <summary>
/// ZendeskCredentials.cs - Core module for the Darl.dev project.
/// </summary>

﻿using System.ComponentModel.DataAnnotations;

namespace DarlCommon
{
    public class ZendeskCredentials
    {
        [Display(Name = "Zendesk URL", Description = "Supplied by Zendesk")]
        [Required]
        public string? ZendeskURL { get; set; }

        [Display(Name = "Zendesk API Key", Description = "Supplied by Zendesk")]
        [Required]
        public string? ZendeskApiKey { get; set; }

        [Display(Name = "Zendesk User name", Description = "Supplied by Zendesk")]
        [Required]
        public string? ZendeskUser { get; set; }
    }
}
