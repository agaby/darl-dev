using Darl.Thinkbase;
using GraphQL.Types;

namespace Darl.GraphQL.Process.Blazor.Schemata
{
    public class GraphObjectUpdateType : InputObjectGraphType<GraphObjectUpdate>
    {
        public GraphObjectUpdateType()
        {
            Name = "graphObjectUpdate";
            Description = "updating an object representing a real world entity in the graph database";
            Field<ListGraphType<DarlTimeInputType>>("existence").Description("The period of existence of the object").Resolve(c => c.Source.existence);
            Field(c => c.id).Description("The id of the object");
            Field(c => c.lineage).Description("The type of this object in the noun hypernymy hierarchy");
            Field(c => c.name, true).Description("The name of this object");
            Field(c => c.externalId, true).Description("An external Id for this object");
            Field(c => c.subLineage, true).Description("An optional secondary lineage");
            Field<ListGraphType<GraphAttributeInputType>>("properties").Description("Other properties of this object").Resolve(c => c.Source.properties);
        }
    }
}
