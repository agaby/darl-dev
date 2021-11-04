using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DarlCommon
{
    public class Connectivity
    {
        [Required]
        [Display(Name = "External connections", Description = "Connections to external SaaS vendors triggered by or triggering projects")]
        public List<Connector>? connectors { get; set; }
    }
}
