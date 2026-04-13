/// </summary>

﻿// ***********************************************************************
// Assembly         : CS.AutomationTest.Web
// Author           : Andrew
// Created          : 11-04-2014
//
// Last Modified By : Andrew
// Last Modified On : 03-12-2015
// ***********************************************************************
// <copyright file="QuestionSetProxy.cs" company="Dr Andy's IP Ltd (BVI)">
//     Copyright ©  2014
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;



namespace DarlCommon
{
    /// The set of questions or responses and status info.
    /// </summary>
    [Serializable]
    public class QuestionSetProxy
    {
        /// Zero or more questions
        /// </summary>
        /// <value>The questions.</value>
        public List<QuestionProxy>? questions { get; set; }

        /// Zero or more responses
        /// </summary>
        /// <value>The responses.</value>        
        public List<ResponseProxy>? responses { get; set; }

        /// Percentage complete, 0-100
        /// </summary>
        /// <value>The percent complete.</value>        
        public double percentComplete { get; set; }

        /// True if questionnaire is completely satisfied.
        /// </summary>
        /// <value><c>true</c> if complete; otherwise, <c>false</c>.</value>        
        public bool complete { get; set; }

        /// Identifies this questionnaire run
        /// </summary>
        /// <value>The ie token.</value>
        [Key]
        public string ieToken { get; set; } = String.Empty;

        /// text displayed before results
        /// </summary>
        /// <value>The response header.</value>       
        public string responseHeader { get; set; } = String.Empty;

        /// text displayed before questions
        /// </summary>
        /// <value>The question header.</value>       
        public string? questionHeader { get; set; }

        /// text displayed before form
        /// </summary>
        /// <value>The preamble.</value>        
        public string? preamble { get; set; }

        /// Indicates that the user can unwind a previous set of answers
        /// </summary>
        /// <value><c>true</c> if this instance can unwind; otherwise, <c>false</c>.</value>        
        public bool canUnwind { get; set; }

        /// Language requested
        /// </summary>
        /// <value>The language.</value>        
        public string? language { get; set; }

        /// The values for reporting, valid if Complete is true.
        /// </summary>
        /// <value>The values.</value>        
        public Dictionary<string, string>? values { get; set; }

        /// Optional request for a set number of questions.
        /// </summary>
        public int questionsRequested { get; set; }

        /// if not empty or null signifies request to redirect to new rule set contained.
        /// </summary>
        public string? redirect { get; set; }
    }
}