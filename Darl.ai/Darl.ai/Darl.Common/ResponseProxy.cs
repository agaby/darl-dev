/// </summary>

﻿// ***********************************************************************
// Assembly         : CS.AutomationTest.Web
// Author           : Andrew
// Created          : 11-04-2014
//
// Last Modified By : Andrew
// Last Modified On : 02-27-2015
// ***********************************************************************
// <copyright file="ResponseProxy.cs" company="Dr Andy's IP Ltd (BVI)">
//     Copyright ©  2014
// </copyright>
// <summary></summary>
// ***********************************************************************


using System;
using System.ComponentModel.DataAnnotations;

namespace DarlCommon
{
    /// Class ResponseProxy.
    /// </summary>
    [Serializable]
    public class ResponseProxy
    {
        /// The types of response possible
        /// </summary>
        public enum RType
        {
            /// initial text to display before questions are asked
            /// </summary>
            Preamble,
            /// a textual response when a result is ready
            /// </summary>
            Text,
            /// The result is expressed as a score bar
            /// </summary>
            ScoreBar,

            /// The result is expressed as a link button
            /// </summary>
            Link
        };

        /// the type of response
        /// </summary>
        /// <value>The rtype.</value> 
        public int rtype { get; set; }
        /// Preamble text
        /// </summary>
        /// <value>The preamble.</value>
        [DataType(DataType.MultilineText)]
        public string? preamble { get; set; }


        /// If text, the second bit of the text, the actual answer
        /// </summary>
        /// <value>The main text.</value>
        /// 
        [DataType(DataType.MultilineText)]
        public string? mainText { get; set; }


        /// The description of the answer
        /// </summary>
        /// <value>The annotation.</value>       
        public string? annotation { get; set; }

        /// A numeric answer value if Score Bar
        /// </summary>
        /// <value>The value.</value>
        public double value { get; set; }
        /// The text to the left of the score bar
        /// </summary>
        /// <value>The low text.</value>

        public string? lowText { get; set; }
        /// the text to the right of the score bar
        /// </summary>
        /// <value>The high text.</value>

        public string? highText { get; set; }
        /// The color of the filled section of the score bar
        /// </summary>
        /// <value>The color.</value>

        public string? color { get; set; }
        /// The value representing 0 on the score bar or the lower possibility bound
        /// </summary>
        /// <value>The minimum value.</value>

        public double minVal { get; set; }
        /// the value representing 100% on the score bar or the upper possibility bound
        /// </summary>
        /// <value>The maximum value.</value>        
        public double maxVal { get; set; }

        /// Gets or sets the format.
        /// </summary>
        /// <value>The format.</value>      
        public string? format { get; set; }
    }
}