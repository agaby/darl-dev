using Darl.Thinkbase;
using GraphQL.Types;

namespace Darl.GraphQL.Process.Blazor.Schemata
{
    public class GraphObjectType : ObjectGraphType<GraphObject>
    {
        public GraphObjectType()
        {
            Name = "graphObject";
            Description = "an object representing a real world entity in the graph database";
            Field<ListGraphType<DarlTimeType>>("existence").Description("The period of existence of the object").Resolve(c => c.Source.existence);
            Field(c => c.id).Description("The unique id");
            Field(c => c.inferred, true).Description("If true, the existence of this object is inferred from other sources");
            Field<StringGraphType>("lineage").Description("The type of this object in the noun hypernymy hierarchy").Resolve(c => ExtractLineage(c.Source.lineage));
            Field<StringGraphType>("subLineage").Description("The sub-type of this object.").Resolve(c => ExtractSubLineage(c.Source.lineage));
            Field(c => c.name, true).Description("The name of this object");
            Field(c => c.externalId, true).Description("A reference to this object in an external system");
            Field<ListGraphType<GraphAttributeType>>("properties").Description("Other properties of this object").Resolve(c => c.Source.properties);
            Field<BooleanGraphType>("virtual").Description("if true the object is a representative of a type, rather than a real world object").Resolve(c => c.Source._virtual);
        }

        private string ExtractLineage(string lineage)
        {
            var pos = lineage.IndexOf('+');
            if (pos == -1)
            {
                return lineage;
            }
            return lineage.Substring(0, pos);
        }
        private string ExtractSubLineage(string lineage)
        {
            var pos = lineage.IndexOf('+');
            if (pos == -1)
            {
                return null;
            }
            return lineage.Substring(pos + 1);
        }
    }
}
