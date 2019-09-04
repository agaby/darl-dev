using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Darl.GraphQL.Models.Models
{
    public class MLSpecUpdate
    {
        [Display(Name = "Darl code")]
        public string darl { get; set; }
        [Display(Name = "Version")]
        public string version { get; set; }
        [Display(Name = "Author")]
        public string author { get; set; }
        [Display(Name = "Copyright")]
        public string copyright { get; set; }
        [Display(Name = "License")]
        public string license { get; set; }
        [Display(Name = "Description")]
        public string description { get; set; }
        [Display(Name = "Training data")]
        public string trainData { get; set; }
        [Display(Name = "Training data schema")]
        public string dataSchema { get; set; }
        [Display(Name = "Training sets")]
        [Range(3, 9)]
        public int? sets { get; set; }
        [Display(Name = "Percentage to test with")]
        [Range(0, 99)]
        public int? percentTest { get; set; }
        [Display(Name = "Trained model to ruleset name")]
        public string destinationRulesetName { get; set; }
    }
}
