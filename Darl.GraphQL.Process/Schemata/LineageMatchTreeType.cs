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
            Name = "LineageMatchTree";
            Description = "A text recognition tree";
            Field(c => c.changed);
            Field<LineageMatchNodeType>("root", resolve: context => context.Source.root);
        }
    }
}
