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
