/// <summary>
/// </summary>

﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Darl.Lineage.Bot
{
    public class BotTestView
    {
        public string conversationID { get; set; }

        [Display(Name = "Conversation")]
        public List<string> conversation { get; set; } = new List<string>();

        [Display(Name = "Generated code")]
        public string darl { get; set; }

        [Display(Name = "Stores data")]
        public List<StoreState> stores { get; set; } = new List<StoreState>();

        public bool reset { get; set; }


        [Required]
        [MaxLength(140)]
        [Display(Name = "Question")]
        public string userText { get; set; }
    }
}
