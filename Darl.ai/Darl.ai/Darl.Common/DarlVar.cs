/// </summary>

﻿// ***********************************************************************
// Assembly         : DarlInfAPI
// Author           : Andrew
// Created          : 05-09-2015
//
// Last Modified By : Andrew
// Last Modified On : 05-14-2015
// ***********************************************************************
// <copyright file="DarlVar.cs" company="Dr Andy's IP Ltd (BVI)">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************

using Darl.Common;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DarlCommon
{
    /// Class DarlVar.
    /// </summary>
    /// <remarks>A general representation of a data value containing related uncertainty information from a fuzzy/possibilistic perspective.</remarks>
    [Serializable]
    [ProtoContract]
    public partial class DarlVar
    {
        /// The type of data stored in the DarlVar
        /// </summary>
        public enum DataType
        {
            /// Numeric including fuzzy
            /// </summary>
            numeric,
            /// One or more categories with confidences
            /// </summary>
            categorical,
            /// Textual
            /// </summary>
            textual,
            /// a text sequence
            /// </summary>
            sequence,
            /// A date with optional time
            /// </summary>
            date,
            /// A time 
            /// </summary>
            time,
            /// a time span
            /// </summary>
            duration,
            /// A geographical location
            /// </summary>
            location,
            /// A url/Uri
            /// </summary>
            link,
            /// The uri of an image
            /// </summary>
            image,
            /// The uri of a video
            /// </summary>
            video,
            /// a username and password
            /// </summary>
            credentials,
            /// A personal name
            /// </summary>
            name,
            /// An organization name
            /// </summary>
            organization,
            /// details of a financial transaction
            /// </summary>
            payment,
            /// details of a ruleset to call
            /// </summary>
            ruleset,
            /// signals a process is complete
            /// </summary>
            complete,
            /// initiates a graph goal seek 
            /// </summary>
            seek,
            /// initiates a graph discovery
            /// </summary>
            discover
        }

        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        [Required]
        [ProtoMember(1)]
        [Display(Name = "The name of the data value", Description = "This should match the name in any associated Darl rule sets")]
        public string name { get; set; } = string.Empty;

        /// This result is unknown if true.
        /// </summary>
        /// <value><c>true</c> if unknown; otherwise, <c>false</c>.</value>
        [Display(Name = "The value is unknown", Description = "If true the value is unknown")]
        [ProtoMember(2)]
        public bool unknown { get; set; } = false;
        /// The confidence placed in this result
        /// </summary>
        /// <value>The weight.</value>
        [Display(Name = "The confidence", Description = "A value identifying the plausibility of the value 0->1")]
        [ProtoMember(3)]
        public double weight { get; set; } = 1.0;

        /// The array containing the up to 4 values representing the fuzzy number.
        /// </summary>
        /// <value>The values.</value>
        /// <remarks>Since all fuzzy numbers used by DARL are convex, i,e. their envelope doesn't have any in-folding
        /// sections, the user can specify numbers with a simple sequence of doubles.
        /// So 1 double represents a crisp or singleton value.
        /// 2 doubles represent an interval,
        /// 3 a triangular fuzzy set,
        /// 4 a trapezoidal fuzzy set.
        /// The values must be ordered in ascending value, but it is permissible for two or more to hold the same value.</remarks>
        [Display(Name = "Set of numeric values", Description = "One to four values describing a fuzzy number if numeric ")]
        [ProtoMember(4)]
        public List<double> values { get; set; }

        /// list of categories, each indexed against a truth value.
        /// </summary>
        /// <value>The categories.</value>
        [Display(Name = "Set of categories", Description = "The possible categories this data item may fall into if categorical, each with confidence value.")]
        [ProtoMember(5)]
        public Dictionary<string, double> categories { get; set; }

        [ProtoMember(6)]
        public List<DarlTime> times { get; set; }

        /// Indicates approximation has taken place in calculating the values.
        /// </summary>
        /// <value><c>true</c> if approximate; otherwise, <c>false</c>.</value>
        /// <remarks>Under some circumstances the coordinates of the fuzzy number
        /// in "values" may not exactly represent the "cuts" values.</remarks>
        [ReadOnly(true)]
        [Display(Name = "Approximated", Description = "True if numeric and fuzzy number processing has resulted in approximation.")]
        [ProtoMember(7)]
        public bool approximate { get; set; }


        /// Gets or sets the type of the data.
        /// </summary>
        /// <value>The type of the data.</value>
        [Required]
        [Display(Name = "The data type", Description = "The kind of data contained")]
        [ProtoMember(8)]
        public DataType dataType { get; set; }

        /// Gets or sets the sequence.
        /// </summary>
        /// <value>The sequence.</value>
        [Display(Name = "The sequence", Description = "A list of ragged array of strings representing a sequence found or to match")]
        public List<List<string>> sequence { get; set; }

        /// Single central or most confident value, expressed as a string or double.
        /// </summary>
        /// <value>The value.</value>
        [Required]
        [Display(Name = "The central value", Description = "The main or dominant value.")]
        [ProtoMember(10)]
        public string Value { get; set; } = string.Empty;

        public override string ToString()
        {
            return Value;
        }

        //exists only to make protobuf happy
        [ProtoMember(9)]
        private List<ProtobufArray> _nestedArrayForProtoBuf // Never used elsewhere
        {
            get
            {
                if (sequence == null)  //  ( _nestedArray == null || _nestedArray.Count == 0 )  if the default constructor instanciate it
                    return null;
                return sequence.Select(p => new ProtobufArray(p)).ToList();
            }
            set
            {
                sequence = value.Select(p => p.InnerArray).ToList();
            }
        }
    }
}