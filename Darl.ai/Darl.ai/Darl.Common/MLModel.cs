/// <summary>
/// MLModel.cs - Core module for the Darl.dev project.
/// </summary>

﻿using System;
using System.ComponentModel.DataAnnotations;

namespace DarlCommon
{
    /// <summary>
    /// specification for supevised machine learning
    /// </summary>
    public class MLModel
    {
        /// <summary>
        /// The darl code of the Machine learning model
        /// </summary>
        [Display(Name = "Darl code")]
        public string darl { get; set; } = String.Empty;

        /// <summary>
        /// The name of the Machine learning model
        /// </summary>
        [Display(Name = "Name")]
        public string name { get; set; } = String.Empty;

        /// <summary>
        /// The version of the Machine learning model
        /// </summary>
        [Display(Name = "Version")]
        public string version { get; set; } = String.Empty;

        /// <summary>
        /// The author of the Machine learning model
        /// </summary>
        [Display(Name = "Author")]
        public string author { get; set; } = String.Empty;

        /// <summary>
        /// The copyright statement of the Machine learning model
        /// </summary>
        [Display(Name = "Copyright")]
        public string copyright { get; set; } = String.Empty;

        /// <summary>
        /// The license for use of the Machine learning model
        /// </summary>
        [Display(Name = "License")]
        public string license { get; set; } = String.Empty;

        /// <summary>
        /// A description of the Machine learning model
        /// </summary>
        [Display(Name = "Description")]
        public string description { get; set; } = String.Empty;

        /// <summary>
        /// training data
        /// </summary>
        [Display(Name = "Training data")]
        public string trainData { get; set; } = String.Empty;

        /// <summary>
        /// training data schema
        /// </summary>
        [Display(Name = "Training data schema")]
        public string dataSchema { get; set; } = String.Empty;

        /// <summary>
        /// Number of sets used in traning
        /// </summary>
        [Display(Name = "Training sets")]
        [Range(3, 9)]
        public int sets { get; set; } //only 3,5,7,9 valid

        /// <summary>
        /// percentage of data to reserve as test.
        /// </summary>
        [Display(Name = "Percentage to test with")]
        [Range(0, 99)]
        public int percentTest { get; set; } //0 - 99

        /// <summary>
        /// The name of the rule set overwritten if the results are saved.
        /// </summary>
        [Display(Name = "Trained model to ruleset name")]
        public string destinationRulesetName { get; set; } = String.Empty;

    }
}
