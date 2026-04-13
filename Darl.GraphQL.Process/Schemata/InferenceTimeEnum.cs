/// <summary>
/// InferenceTimeEnum.cs - Core module for the Darl.dev project.
/// </summary>

﻿using GraphQL.Types;
using static Darl.Thinkbase.IGraphModel;

namespace Darl.GraphQL.Models.Schemata
{
    public class InferenceTimeEnum : EnumerationGraphType<InferenceTime>
    {
        public InferenceTimeEnum()
        {
            Name = "InferenceTime";
            Description = "Determines if inferences are performed using the current time or some fixed time.";
        }

    }
}
