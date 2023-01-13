using DarlCommon;
using GraphQL.Types;

namespace Darl.GraphQL.Process.Blazor.Schemata
{
    public class DaslSetInputType : InputObjectGraphType<DaslSet>
    {
        public DaslSetInputType()
        {
            Name = "daslSetInput";
            Description = "A contact that has requested to be informed about DARL.ai";
            Description = "A description of a system over time";
            Field(c => c.description, true).Description("text describing this data set");
            Field(c => c.sampleTime).Description("The sample time to use in analysis and prediction");
            Field<ListGraphType<DaslStateInputType>>("events").Description("The set of states describing the system over time").Resolve(context => context.Source.events);
        }
    }
}
