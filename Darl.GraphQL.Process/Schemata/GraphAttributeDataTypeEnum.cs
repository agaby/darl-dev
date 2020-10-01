using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;
using static Darl.Thinkbase.GraphAttribute;

namespace Darl.GraphQL.Models.Schemata
{
    public class GraphAttributeDataTypeEnum : EnumerationGraphType<DataType>
    {
        public GraphAttributeDataTypeEnum()
        {
            Name = "dataTypes";
            Description = "The data types of an attribute.";
        }
    }
}
