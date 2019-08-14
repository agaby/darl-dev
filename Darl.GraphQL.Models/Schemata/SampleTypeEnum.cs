using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class SampleTypeEnum : EnumerationGraphType
    {

        public SampleTypeEnum()
        {
            Name = "sampleType";
            AddValue("events", "The data consists of irregular events", 0);
            AddValue("sampled", "The data consists of regularly sampled items", 1);
        }
    }
}
