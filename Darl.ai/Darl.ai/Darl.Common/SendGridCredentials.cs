/// <summary>
/// SendGridCredentials.cs - Core module for the Darl.dev project.
/// </summary>

﻿using System.ComponentModel.DataAnnotations;

namespace DarlCommon
{
    public class SendGridCredentials
    {
        [Display(Name = "SendGrid API Key", Description = "Supplied by SendGrid")]
        [Required]
        public string SendGridAPIKey { get; set; }
    }
}
