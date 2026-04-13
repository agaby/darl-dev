/// <summary>
/// </summary>

﻿using DarlLanguage.Processing;
using System;
using System.Collections.Generic;

namespace Darl.Forms
{
    public class QuestionCache
    {
        /// <summary>
        /// Identifies the attached Questionnaire
        /// </summary>
        public Guid SessionKey;

        /// <summary>
        /// Identifies the source combination;
        /// </summary>
        public string? projectId;

        /// <summary>
        /// Current set of name value pairs
        /// </summary>
        /// <value>The current data.</value>
        public List<DarlResult> currentData { get; set; } = new List<DarlResult>();

        /// <summary>
        /// count of inputs needing data at start of processing
        /// </summary>
        /// <value>The total reachable inputs.</value>
        public int totalReachableInputs { get; set; } = 0;

        /// <summary>
        /// Current iteration, initially 0
        /// </summary>
        /// <value>The current iteration.</value>
        public int currentIteration { get; set; } = 0;

        /// <summary>
        /// Language selection for this session
        /// </summary>
        /// <value>The language selection.</value>
        public string? languageSelection { get; set; }

        /// <summary>
        /// Gets or sets the tenant.
        /// </summary>
        /// <value>
        /// The tenant.
        /// </value>
        public string? tenant { get; set; }

        /// <summary>
        /// Gets or sets the requested question count.
        /// </summary>
        /// <value>
        /// The requested question count.
        /// </value>
        public int requestedQuestions { get; set; } = 1;

        /// <summary>
        /// If this rule set has been called by another the id is contained here, otherwise empty
        /// </summary>
        public string callingRuleSet { get; set; } = string.Empty; //ultimately will become Stack<string>

        /// <summary>
        /// A set of local stores if used
        /// </summary>
        public Dictionary<string, ILocalStore>? stores { get; set; }
    }
}