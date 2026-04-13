/// </summary>

﻿using System;
using System.ComponentModel.DataAnnotations;

namespace DarlCommon
{
    /// specification for supevised machine learning
    /// </summary>
    public class MLModel
    {
        /// The darl code of the Machine learning model
        /// </summary>
        [Display(Name = "Darl code")]
        public string darl { get; set; } = String.Empty;

        /// The name of the Machine learning model
        /// </summary>
        [Display(Name = "Name")]
        public string name { get; set; } = String.Empty;

        /// The version of the Machine learning model
        /// </summary>
        [Display(Name = "Version")]
        public string version { get; set; } = String.Empty;

        /// The author of the Machine learning model
        /// </summary>
        [Display(Name = "Author")]
        public string author { get; set; } = String.Empty;

        /// The copyright statement of the Machine learning model
        /// </summary>
        [Display(Name = "Copyright")]
        public string copyright { get; set; } = String.Empty;

        /// The license for use of the Machine learning model
        /// </summary>
        [Display(Name = "License")]
        public string license { get; set; } = String.Empty;

        /// A description of the Machine learning model
        /// </summary>
        [Display(Name = "Description")]
        public string description { get; set; } = String.Empty;

        /// training data
        /// </summary>
        [Display(Name = "Training data")]
        public string trainData { get; set; } = String.Empty;

        /// training data schema
        /// </summary>
        [Display(Name = "Training data schema")]
        public string dataSchema { get; set; } = String.Empty;

        /// Number of sets used in traning
        /// </summary>
        [Display(Name = "Training sets")]
        [Range(3, 9)]
        public int sets { get; set; } //only 3,5,7,9 valid

        /// percentage of data to reserve as test.
        /// </summary>
        [Display(Name = "Percentage to test with")]
        [Range(0, 99)]
        public int percentTest { get; set; } //0 - 99

        /// The name of the rule set overwritten if the results are saved.
        /// </summary>
        [Display(Name = "Trained model to ruleset name")]
        public string destinationRulesetName { get; set; } = String.Empty;

    }
}
