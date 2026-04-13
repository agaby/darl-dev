/// </summary>

﻿// ***********************************************************************
// Assembly         : DarlCompiler
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="ParserTrace.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;

namespace DarlCompiler.Parsing
{
    /// Class ParserTraceEntry.
    /// </summary>
    public class ParserTraceEntry
    {
        /// The state
        /// </summary>
        public ParserState State;
        /// The stack top
        /// </summary>
        public ParseTreeNode StackTop;
        /// The input
        /// </summary>
        public ParseTreeNode Input;
        /// The message
        /// </summary>
        public string Message;
        /// The is error
        /// </summary>
        public bool IsError;

        /// Initializes a new instance of the <see cref="ParserTraceEntry"/> class.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="stackTop">The stack top.</param>
        /// <param name="input">The input.</param>
        /// <param name="message">The message.</param>
        /// <param name="isError">if set to <c>true</c> [is error].</param>
        public ParserTraceEntry(ParserState state, ParseTreeNode stackTop, ParseTreeNode input, string message, bool isError)
        {
            State = state;
            StackTop = stackTop;
            Input = input;
            Message = message;
            IsError = isError;
        }
    }

    /// Class ParserTrace.
    /// </summary>
    public class ParserTrace : List<ParserTraceEntry> { }

    /// Class ParserTraceEventArgs.
    /// </summary>
    public class ParserTraceEventArgs : EventArgs
    {
        /// Initializes a new instance of the <see cref="ParserTraceEventArgs"/> class.
        /// </summary>
        /// <param name="entry">The entry.</param>
        public ParserTraceEventArgs(ParserTraceEntry entry)
        {
            Entry = entry;
        }

        /// The entry
        /// </summary>
        public readonly ParserTraceEntry Entry;

        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return Entry.ToString();
        }
    }



}
