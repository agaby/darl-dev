using DarlCommon;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class DaslSetType : ObjectGraphType<DaslSet>
    {

        public DaslSetType()
        {
            Name = "daslSet";
            Description = "A description of a system over time";
            Field(c => c.description).Description("text describing this data set");
            Field(c => c.sampleTime).Description("The sample time to use in analysis and prediction");
            Field<ListGraphType<DaslStateType>>("events", "The set of states describing the system over time", resolve: context => context.Source.events);
        }
    }
}
