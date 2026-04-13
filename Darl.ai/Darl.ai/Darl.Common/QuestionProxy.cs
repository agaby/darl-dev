/// </summary>

﻿// ***********************************************************************
// Assembly         : CS.AutomationTest.Web
// Author           : Andrew
// Created          : 11-04-2014
//
// Last Modified By : Andrew
// Last Modified On : 02-27-2015
// ***********************************************************************
// <copyright file="QuestionProxy.cs" company="Dr Andy's IP Ltd (BVI)">
//     Copyright ©  2014
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;


namespace DarlCommon
{
    /// Class QuestionProxy.
    /// </summary>
    [Serializable]
    public class QuestionProxy
    {
        /// Used to define the data type
        /// </summary>
        public enum QType
        {
            /// a numeric answer is expected
            /// </summary>
            numeric,
            /// a categorical answer is expected
            /// </summary>
            categorical,
            /// a textual answer is expected
            /// </summary>
            textual,
            /// A temporal answer is expected
            /// </summary>
            temporal
        };
        /// The id of the question
        /// </summary>
        /// <value>The reference.</value>

        public string reference { get; set; } = String.Empty;
        /// The text of the question
        /// </summary>
        /// <value>The text.</value>

        public string text { get; set; } = String.Empty;

        /// Gets or sets the path.
        /// </summary>
        /// <value>The path.</value>

        public string path { get; set; } = String.Empty;

        /// Gets or sets the format.
        /// </summary>
        /// <value>The format.</value>

        public string format { get; set; } = String.Empty;

        /// The type of the question
        /// </summary>
        /// <value>The qtype.</value>

        public int qtype { get; set; }
        /// If numeric the lower bound (-infinity if unbounded)
        /// </summary>
        /// <value>The minval.</value>

        public double minval { get; set; }

        /// If numeric the upper bound (+infinity if unbounded)
        /// </summary>
        /// <value>The maxval.</value>

        public double maxval { get; set; }

        /// The increment to use for number selection - 1 for integers, 0 for continuum etc.
        /// </summary>
        /// <value>The increment.</value>

        public double increment { get; set; }

        /// A list of permissible categories if categorical
        /// </summary>
        /// <value>The categories.</value>

        public List<string>? categories { get; set; }

        /// String responses
        /// </summary>
        /// <value>The s response.</value>

        public string sResponse { get; set; } = String.Empty;

        /// numeric response
        /// </summary>
        /// <value>The d response.</value>

        public double dResponse { get; set; }

        /// Gets or sets a value indicating whether only one category or a crisp numeric value is required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enforce crisp]; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>This is a hint to the UI. </remarks>
        public bool enforceCrisp { get; set; }
    }
}
