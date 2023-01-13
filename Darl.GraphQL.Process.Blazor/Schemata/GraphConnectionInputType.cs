using Darl.Thinkbase;
using GraphQL.Types;

namespace Darl.GraphQL.Process.Blazor.Schemata
{
    public class GraphConnectionInputType : InputObjectGraphType<GraphConnectionInput>
    {
        public GraphConnectionInputType()
        {
            Name = "graphConnectionInput";
            Description = "A connection representing a real world relationship in the graph database";
            Field<ListGraphType<DarlTimeInputType>>("existence").Description("The period of existence of the connection").Resolve(c => c.Source.existence);
            Field(c => c.lineage).Description("The type of this connection in the verb hypernymy hierarchy");
            Field(c => c.name).Description("The name of this connection");
            Field(c => c.startId).Description("The object at the start of this connection");
            Field(c => c.endId).Description("The object at the end of this connection");
            Field(c => c.weight, true).Description("The degree of plausibility of this connection").DefaultValue(1.0);
            Field(c => c.id, true).Description("The id of this connection");//cytoscape sets this...
            Field<ListGraphType<GraphAttributeInputType>>("properties").Description("Other properties of this connection").Resolve(c => c.Source.properties);
        }
    }
}
