using Darl.Thinkbase;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class GraphAttributeInputType : InputObjectGraphType<GraphAttribute>
    {
        public GraphAttributeInputType()
        {
            Name = "graphAttributeInput";
            Description = "An attribute of an object or connection";
            Field<ListGraphType<DateTimeGraphType>>("existence", "The period of existence of the connection", resolve: c => c.Source.existence);
            Field(c => c.lineage).Description("The type of this connection in the verb hypernymy hierarchy");
            Field(c => c.name,true).Description("The name of this connection");
            Field(c => c.value,true).Description("The object at the start of this connection");
            Field(c => c.type,true).Description("The object at the end of this connection");
            Field(c => c.confidence, true).Description("The degree of plausibility of this connection").DefaultValue(1.0);
        }
    }
}
