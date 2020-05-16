using System.Collections.Generic;

namespace Darl.GraphQL.Models.Models
{
    public class KGTrainingValue
    {
        /// <summary>
        /// The properties containing the text to match
        /// </summary>
        public List<string> valueProperty { get; set; }

        /// <summary>
        /// The values to match
        /// </summary>
        public List<List<string>> values { get; set; }

        /// <summary>
        /// The lineages selecting the set of objects
        /// </summary>
        public List<string> valueLineages { get; set; } = new List<string>();

        /// <summary>
        /// The graph to hold the trained model
        /// </summary>
        public MatchGraph graph { get; set; }

        /// <summary>
        /// If true this is the index - only one index allowed.
        /// </summary>
        public bool index { get; set; } = false;
    }
}