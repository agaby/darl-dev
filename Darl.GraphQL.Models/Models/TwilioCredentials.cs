using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Darl.GraphQL.Models.Models
{
    public class TwilioCredentials
    {
        [Display(Name = "Twilio SID", Description = "Supplied by Twilio")]
        [Required]
        public string SMSAccountIdentification { get; set; }
        [Display(Name = "Twilio password", Description = "Supplied by Twilio")]
        [Required]
        public string SMSAccountPassword { get; set; }
        [Display(Name = "Sending number", Description = "Phone number supplied by Twilio")]
        [Required]
        public string SMSAccountFrom { get; set; }
    }
}
