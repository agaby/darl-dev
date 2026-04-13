/// <summary>
/// Connector.cs - Core module for the Darl.dev project.
/// </summary>

﻿using System.ComponentModel.DataAnnotations;

namespace DarlCommon
{
    public enum ConnectorType { twilio, sendgrid, skype, facebook, twitter, linkedin, custom, project, zendesk, microsoft_bot }
    public class Connector
    {
        [Required]
        [Display(Name = "Name", Description = "Unique reference name for this connection")]
        public string name { get; set; } = string.Empty;

        [Required]
        [Display(Name = "SaaS vendor", Description = "The SaaS service you are connecting to.")]
        public ConnectorType? type { get; set; }

        [Required]
        [Display(Name = "Your ID", Description = "The SaaS service ID that identifies your account")]
        public string? id { get; set; }

        [Required]
        [Display(Name = "Your API key", Description = "The SaaS service secret token or API key")]
        public string? key { get; set; }

        [Required]
        [Display(Name = "The destination address", Description = "The destination of any transmission over a public medium")]
        public string? dest { get; set; }

        [Required]
        [Display(Name = "The source address", Description = "The declared source of any transmission over a public medium")]
        public string? src { get; set; }
    }
}