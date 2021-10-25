using System;
using System.ComponentModel.DataAnnotations;

namespace DarlCommon
{
    public class ProjectRecordView
    {
        public string subdomain { get; set; }
        public string id { get; set; }
        [Display(Name = "Name of the project")]
        public string name { get; set; }
        public string source1 { get; set; }
        public string source2 { get; set; }
        public string source3 { get; set; }
        public string source4 { get; set; }
        [Display(Name = "Public")]
        public bool publicView { get; set; }
        [Display(Name = "Read only")]
        public bool readonlyView { get; set; }
        public int prType { get; set; }
        [Display(Name = "Description of the project")]
        public string description { get; set; }
        [Display(Name = "I/O to monitor")]
        public string monitor { get; set; }
        [Display(Name = "Sets for numeric variables")]
        public int sets { get; set; }
        [Display(Name = "Percentage to train on")]
        [Range(1, 100)]
        public int percentTrain { get; set; }
    }
}
