using Darl.Lineage;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class LineageMatchTreeType : ObjectGraphType<LineageMatchTree>
    {
        public LineageMatchTreeType()
        {
            Field(c => c.changed);
            Field(c => c.root);
        }
    }
}
