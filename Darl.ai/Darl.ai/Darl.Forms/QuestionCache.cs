/// </summary>

﻿using DarlLanguage.Processing;
using System;
using System.Collections.Generic;

namespace Darl.Forms
{
    public class QuestionCache
    {
        /// Identifies the attached Questionnaire
        /// </summary>
        public Guid SessionKey;

        /// Identifies the source combination;
        /// </summary>
        public string? projectId;

        /// Current set of name value pairs
        /// </summary>
        /// <value>The current data.</value>
        public List<DarlResult> currentData { get; set; } = new List<DarlResult>();

        /// count of inputs needing data at start of processing
        /// </summary>
        /// <value>The total reachable inputs.</value>
        public int totalReachableInputs { get; set; } = 0;

        /// Current iteration, initially 0
        /// </summary>
        /// <value>The current iteration.</value>
        public int currentIteration { get; set; } = 0;

        /// Language selection for this session
        /// </summary>
        /// <value>The language selection.</value>
        public string? languageSelection { get; set; }

        /// Gets or sets the tenant.
        /// </summary>
        /// <value>
        /// The tenant.
        /// </value>
        public string? tenant { get; set; }

        /// Gets or sets the requested question count.
        /// </summary>
        /// <value>
        /// The requested question count.
        /// </value>
        public int requestedQuestions { get; set; } = 1;

        /// If this rule set has been called by another the id is contained here, otherwise empty
        /// </summary>
        public string callingRuleSet { get; set; } = string.Empty; //ultimately will become Stack<string>

        /// A set of local stores if used
        /// </summary>
        public Dictionary<string, ILocalStore>? stores { get; set; }
    }
}