/// </summary>

﻿using System.ComponentModel.DataAnnotations;

namespace DarlCommon
{
    public class TextView
    {
        [Display(Name = "Sentence")]
        public string text { get; set; }

        public string svg { get; set; }
    }
}
