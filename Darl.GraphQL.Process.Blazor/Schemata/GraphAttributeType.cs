using Darl.Thinkbase;
using GraphQL.Types;

namespace Darl.GraphQL.Process.Blazor.Schemata
{
    public class GraphAttributeType : ObjectGraphType<GraphAttribute>
    {
        public GraphAttributeType()
        {
            Name = "graphAttribute";
            Description = "An attribute of an object or connection";
            Field<ListGraphType<DarlTimeType>>("existence").Description("The period of existence of the object").Resolve(c => c.Source.existence);
            Field(c => c.id).Description("The unique id");
            Field(c => c.inferred, true).Description("If true, the existence of this object is inferred from other sources");
            Field(c => c.lineage).Description("The type of this object in the noun hypernymy hierarchy");
            Field(c => c.name, true).Description("The name of this object");
            Field(c => c.value, true).Description("The value of this object");
            Field(c => c.confidence, true).Description("The confidence of this object");
            Field<GraphAttributeDataTypeEnum>("type").Description("The type of this object").Resolve(c => c.Source.type);
            Field<BooleanGraphType>("virtual").Description("if true the object is a representative of a type, rather than a real world object").Resolve(c => c.Source._virtual);
            Field<ListGraphType<GraphAttributeType>>("properties").Description("sub-attributes of this attribute").Resolve(c => c.Source.properties);
        }
    }
}
