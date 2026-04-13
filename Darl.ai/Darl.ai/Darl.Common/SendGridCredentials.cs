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
