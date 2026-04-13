/// <summary>
/// </summary>

﻿using Darl.Thinkbase;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class DataMapType : InputObjectGraphType<DataMap>
    {
        public DataMapType()
        {
            Name = "dataMap";
            Description = "A mapping between a single data item's relative xPath or jPath and the KG attribute it refers to.";
            Field(c => c.relPath).Description("The XPath or JPath path relative to he pattern path.");
            Field(c => c.objId).Description("The object ID or external ID of the node to recieve the data item.");
            Field(c => c.attLineage, true).Description("The lineage of the attribute to receive the data item.");
            Field(c => c.target, true).Description("Flags that this field is the supervised learning target, i.e. the thing to learn.");
            Field(c => c.objectLineage, true).Description("The lineage to be given to a created node for this field.");
            Field(c => c.objectSubLineage, true).Description("The sub-lineage to be given to a created node for this field.");
            Field<GraphAttributeDataTypeEnum>("dataType", "The data type of the field", resolve: c => c.Source.dataType);
        }

    }
}
