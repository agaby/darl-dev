/// <summary>
/// TwilioCredentials.cs - Core module for the Darl.dev project.
/// </summary>

﻿using System.ComponentModel.DataAnnotations;

namespace DarlCommon
{
    public class TwilioCredentials
    {
        [Display(Name = "Twilio SID", Description = "Supplied by Twilio")]
        [Required]
        public string? SMSAccountIdentification { get; set; }
        [Display(Name = "Twilio password", Description = "Supplied by Twilio")]
        [Required]
        public string? SMSAccountPassword { get; set; }
        [Display(Name = "Sending number", Description = "Phone number supplied by Twilio")]
        [Required]
        public string? SMSAccountFrom { get; set; }
    }
}
