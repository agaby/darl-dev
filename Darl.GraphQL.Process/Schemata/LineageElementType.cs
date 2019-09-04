using Darl.Lineage;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class LineageElementType : ObjectGraphType<LineageElement>
    {
        public LineageElementType()
        {
            Name = "LineageElement";
            Description = "A node of a hypernymy tree";
            Field(c => c.description);
            Field(c => c.lineage);
            Field<LineageTypeEnum>("lineageType", resolve: c => c.Source.type);
        }
    }
}
