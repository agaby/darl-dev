using Darl.Thinkbase;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class GraphTypeEnum : EnumerationGraphType<GraphElementType>
    {
        public GraphTypeEnum()
        {
            Name = "graphElementTypes";
            Description = "The kinds of graph elements";
        }
    }
}
