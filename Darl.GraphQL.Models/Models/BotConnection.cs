using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Darl.GraphQL.Models.Models
{
    public class BotConnection
    {
        [Display(Name = "BotFramework AppId", Description = "This holds the AppID assigned by the MS Bot framework for this bot")]
        [Required]
        public string AppId { get; set; }

        [Display(Name = "BotFramework password", Description = "This holds the password assigned by the MS Bot framework for this bot")]
        [DataType(DataType.Password)]
        [Required]
        public string Password { get; set; }

        public string FreindlyName { get; set; }

        public List<BotUsage> UsageHistory { get; set; } = new List<BotUsage>();
    }
}
