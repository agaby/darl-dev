/// <summary>
/// StructureException.cs - Core module for the Darl.dev project.
/// </summary>

﻿using System;
using System.Runtime.Serialization;

namespace Darl.Thinkbase.Meta
{
    /// <summary>
    /// Indicates a design fault discovered in the knowledge graph
    /// </summary>
    public class StructureException : Exception
    {
        public StructureException(string message) : base(message)
        {
        }

        public StructureException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected StructureException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
