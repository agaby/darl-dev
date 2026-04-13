/// <summary>
/// SampleTypeEnum.cs - Core module for the Darl.dev project.
/// </summary>

﻿using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class SampleTypeEnum : EnumerationGraphType
    {

        public SampleTypeEnum()
        {
            Name = "sampleType";
            Add("events", 0, "The data consists of irregular events");
            Add("sampled", 1, "The data consists of regularly sampled items");
        }
    }
}
