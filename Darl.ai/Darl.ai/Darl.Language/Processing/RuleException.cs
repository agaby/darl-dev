/// </summary>

﻿using System;

namespace DarlLanguage.Processing
{
    /// A ruleset specific exception
    /// </summary>
    [Serializable]
    public class RuleException : Exception
    {
        /// Initializes a RuleException object
        /// </summary>
        public RuleException()
            : base() { }
        /// Initializes a RuleException object
        /// </summary>
        /// <param name="message">Text to pass with the exception</param>
        public RuleException(String message)
            : base(message) { }
        /// Initializes a RuleException object
        /// </summary>
        /// <param name="message">Text to pass with the exception</param>
        /// <param name="innerException">An exception that caused this exception, passed for information</param>
        public RuleException(String message, Exception innerException)
            : base(message, innerException) { }
    }
}
