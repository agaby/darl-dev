using Darl.GraphQL.Models.Models;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class LineageMatchNodePairType : ObjectGraphType<LineageMatchNodePair>
    {
        public LineageMatchNodePairType()
        {
            Field(c => c.Text);
            Field<LineageMatchNodeType>("Match", resolve: c => c.Source.Match);
        }
    }
}
