/// <summary>
/// GraphAttributeDataTypeEnum.cs - Core module for the Darl.dev project.
/// </summary>

﻿using GraphQL.Types;
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
