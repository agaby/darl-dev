using System;
using System.Collections.Generic;

namespace DarlCommon
{
    /// <summary>
    /// Interchange format for Darl questionnaires
    /// </summary>
    /// <remarks>Transferred as Json with enums as strings. File extension is ".rule"</remarks>
    [Serializable]
    public class RuleForm
    {
        /// <summary>
        /// The darl code of the questionnaire
        /// </summary>
        public string? darl { get; set; }

        /// <summary>
        /// The name of the questionnaire
        /// </summary>
        public string? name { get; set; }

        /// <summary>
        /// The version of the questionnaire
        /// </summary>
        public string? version { get; set; }

        /// <summary>
        /// The author of the questionnaire
        /// </summary>
        public string? author { get; set; }

        /// <summary>
        /// The copyright statement of the questionnaire
        /// </summary>
        public string? copyright { get; set; }

        /// <summary>
        /// The license for use of the questionnaire
        /// </summary>
        public string? license { get; set; }

        /// <summary>
        /// A description of the questionnaire
        /// </summary>
        public string? description { get; set; }

        /// <summary>
        /// The format of the I/O of the questionnaire
        /// </summary>
        public FormFormat? format { get; set; }

        /// <summary>
        /// The texts used in the questionnaire
        /// </summary>
        public LanguageFormat? language { get; set; }

        /// <summary>
        /// External events triggered on rule set completion
        /// </summary>
        public TriggerView? trigger { get; set; }

        /// <summary>
        /// Preloaded data, such as text formats
        /// </summary>
        public List<DarlVar>? preload { get; set; }

        /// <summary>
        /// An image relating to the rule set for a directory
        /// </summary>
        public string? imageUrl { get; set; }

        /// <summary>
        /// Price per use
        /// </summary>
        public double price { get; set; } = 0.0;

        /// <summary>
        /// Curreny of price in ISO format.
        /// </summary>
        public string currency { get; set; } = "USD";

        /// <summary>
        /// testing and validation data
        /// </summary>
        public string? testData { get; set; }

        public List<string>? storeNames { get; set; }

    }
}
