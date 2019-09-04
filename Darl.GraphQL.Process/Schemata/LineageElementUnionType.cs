using Darl.Lineage;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

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
