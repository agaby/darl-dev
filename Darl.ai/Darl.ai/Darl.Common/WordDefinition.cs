/// </summary>

﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DarlCommon
{
    public class WordDefinition
    {
        [Display(Name = "Word")]
        public string? word { get; set; }
        [Display(Name = "Dominant part of speech")]
        public string? dominantType { get; set; }
        public List<ConceptDefinition>? definitions { get; set; }
    }
}
