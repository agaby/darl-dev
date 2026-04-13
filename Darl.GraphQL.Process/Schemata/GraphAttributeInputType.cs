/// <summary>
/// </summary>

﻿using Darl.Thinkbase;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class GraphAttributeInputType : InputObjectGraphType<GraphAttributeInput>
    {
        public GraphAttributeInputType()
        {
            Name = "graphAttributeInput";
            Description = "An attribute of an object or connection";
            Field<ListGraphType<DarlTimeInputType>>("existence", "The period of existence of the connection", resolve: c => c.Source.existence);
            Field(c => c.lineage).Description("The type of this attribute in the hypernymy hierarchy");
            Field(c => c.subLineage, true).Description("The sub-type of this attribute in the hypernymy hierarchy");
            Field(c => c.name, true).Description("The name of this Attribute");
            Field(c => c.value, true).Description("The value of this attribute");
            Field<GraphAttributeDataTypeEnum>("type", "The type of this attribute", resolve: c => c.Source.type);
            Field(c => c.confidence, true).Description("The degree of plausibility of this attribute").DefaultValue(1.0);
            Field(c => c.inferred, true).Description("If true, the existence of this object is inferred from other sources");
            Field<ListGraphType<GraphAttributeInputType>>("properties", "sub-attributes of this attribute", resolve: c => c.Source.properties);
        }
    }
}
