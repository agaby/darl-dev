/// </summary>

﻿using Darl.SoftMatch;
using System.Collections.Generic;

namespace Darl.GraphQL.Models.Models
{
    public class KGTrainingValue
    {
        /// The properties containing the text to match
        /// </summary>
        public List<string> valueProperty { get; set; }

        /// The values to match
        /// </summary>
        public List<List<string>> values { get; set; }

        /// The lineages selecting the set of objects
        /// </summary>
        public List<string> valueLineages { get; set; } = new List<string>();

        /// The graph to hold the trained model
        /// </summary>
        public MatchList graph { get; set; }

        /// If true this is the index - only one index allowed.
        /// </summary>
        public bool index { get; set; } = false;
    }
}