using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DarlCommon
{
    public class TextView
    {
        [Display(Name = "Sentence")]
        public string text { get; set; }

        public string svg { get; set; }
    }
}
