/// </summary>

﻿using System;
using System.Collections.Generic;

namespace DarlCommon
{
    /// Interchange format for Darl questionnaires
    /// </summary>
    /// <remarks>Transferred as Json with enums as strings. File extension is ".rule"</remarks>
    [Serializable]
    public class RuleForm
    {
        /// The darl code of the questionnaire
        /// </summary>
        public string? darl { get; set; }

        /// The name of the questionnaire
        /// </summary>
        public string? name { get; set; }

        /// The version of the questionnaire
        /// </summary>
        public string? version { get; set; }

        /// The author of the questionnaire
        /// </summary>
        public string? author { get; set; }

        /// The copyright statement of the questionnaire
        /// </summary>
        public string? copyright { get; set; }

        /// The license for use of the questionnaire
        /// </summary>
        public string? license { get; set; }

        /// A description of the questionnaire
        /// </summary>
        public string? description { get; set; }

        /// The format of the I/O of the questionnaire
        /// </summary>
        public FormFormat? format { get; set; }

        /// The texts used in the questionnaire
        /// </summary>
        public LanguageFormat? language { get; set; }

        /// External events triggered on rule set completion
        /// </summary>
        public TriggerView? trigger { get; set; }

        /// Preloaded data, such as text formats
        /// </summary>
        public List<DarlVar>? preload { get; set; }

        /// An image relating to the rule set for a directory
        /// </summary>
        public string? imageUrl { get; set; }

        /// Price per use
        /// </summary>
        public double price { get; set; } = 0.0;

        /// Curreny of price in ISO format.
        /// </summary>
        public string currency { get; set; } = "USD";

        /// testing and validation data
        /// </summary>
        public string? testData { get; set; }

        public List<string>? storeNames { get; set; }

    }
}
