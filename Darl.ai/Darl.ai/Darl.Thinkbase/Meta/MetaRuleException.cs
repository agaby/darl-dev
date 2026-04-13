/// <summary>
/// MetaRuleException.cs - Core module for the Darl.dev project.
/// </summary>

﻿using System;
using System.Runtime.Serialization;

namespace Darl.Thinkbase.Meta
{
    [Serializable]
    public class MetaRuleException : Exception
    {
        public MetaRuleException()
        {
        }

        public MetaRuleException(string message) : base(message)
        {
        }

        public MetaRuleException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected MetaRuleException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}