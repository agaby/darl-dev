using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Darl.GraphQL.Models.Models
{
    public class AzureCredentials
    {
        [Display(Name = "Azure storage API Key", Description = "Supplied by Microsoft Azure")]
        [Required]
        public string AzureAPIKey { get; set; }
    }
}
