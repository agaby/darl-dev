using Darl.Thinkbase;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class GraphAttributeType : ObjectGraphType<GraphAttribute>
    {
        public GraphAttributeType()
        {
            Name = "graphAttribute";
            Description = "An attribute of an object or connection";
            Field<ListGraphType<DarlTimeType>>("existence", "The period of existence of the object", resolve: c => c.Source.existence);
            Field(c => c.id).Description("The unique id");
            Field(c => c.inferred, true).Description("If true, the existence of this object is inferred from other sources");
            Field(c => c.lineage).Description("The type of this object in the noun hypernymy hierarchy");
            Field(c => c.name, true).Description("The name of this object");
            Field(c => c.value, true).Description("The value of this object");
            Field(c => c.confidence, true).Description("The confidence of this object");
            Field<GraphAttributeDataTypeEnum>("type", "The type of this object", resolve: c => c.Source.type);
            Field<BooleanGraphType>("virtual", "if true the object is a representative of a type, rather than a real world object", resolve: c => c.Source._virtual);
        }
    }
}
