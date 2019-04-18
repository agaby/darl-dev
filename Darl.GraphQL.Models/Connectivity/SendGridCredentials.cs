using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Darl.GraphQL.Models.Connectivity
{
    public class SendGridCredentials
    {
        [Display(Name = "SendGrid API Key", Description = "Supplied by SendGrid")]
        [Required]
        public string SendGridAPIKey { get; set; }
    }
}
