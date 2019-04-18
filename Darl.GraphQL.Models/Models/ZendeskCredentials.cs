using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Darl.GraphQL.Models.Models
{
    public class ZendeskCredentials
    {
        [Display(Name = "Zendesk URL", Description = "Supplied by Zendesk")]
        [Required]
        public string ZendeskURL { get; set; }

        [Display(Name = "Zendesk API Key", Description = "Supplied by Zendesk")]
        [Required]
        public string ZendeskApiKey { get; set; }

        [Display(Name = "Zendesk User name", Description = "Supplied by Zendesk")]
        [Required]
        public string ZendeskUser { get; set; }
    }
}
