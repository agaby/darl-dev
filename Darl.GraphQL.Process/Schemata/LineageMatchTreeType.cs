/// <summary>
/// LineageMatchTreeType.cs - Core module for the Darl.dev project.
/// </summary>

﻿using Darl.Lineage;
using GraphQL.Types;

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
