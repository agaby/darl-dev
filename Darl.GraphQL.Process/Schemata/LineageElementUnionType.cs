/// <summary>
/// LineageElementUnionType.cs - Core module for the Darl.dev project.
/// </summary>

﻿using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class LineageElementUnionType : UnionGraphType
    {
        public LineageElementUnionType()
        {
            Type<LineageElementType>();
            Type<LineageRecordType>();
        }
    }
}
