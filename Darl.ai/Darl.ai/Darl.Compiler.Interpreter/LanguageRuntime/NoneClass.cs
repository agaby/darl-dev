/// </summary>

﻿// ***********************************************************************
// Assembly         : DarlCompiler.Interpreter
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="NoneClass.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using Darl.ai;

namespace DarlCompiler.Interpreter
{

    // A class for special reserved None value used in many scripting languages. 
    /// Class NoneClass.
    /// </summary>
    public class NoneClass
    {
        /// The _to string
        /// </summary>
        readonly string _toString;

        /// Prevents a default instance of the <see cref="NoneClass"/> class from being created.
        /// </summary>
        private NoneClass()
        {
            _toString = Resources.LabelNone;
        }
        /// Initializes a new instance of the <see cref="NoneClass"/> class.
        /// </summary>
        /// <param name="toString">To string.</param>
        public NoneClass(string toString)
        {
            _toString = toString;
        }
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return _toString;
        }

        /// The value
        /// </summary>
        public static NoneClass Value = new NoneClass();
    }



}
