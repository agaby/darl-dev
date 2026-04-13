/// </summary>

﻿using Darl.Lineage;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class LineageAnnotationNodeType : ObjectGraphType<LineageAnnotationNode>
    {
        public LineageAnnotationNodeType()
        {
            Name = "LineageAnnotationNode";
            Description = "Annotation to define a node as terminal and define the actions associated with it.";
            Field<ListGraphType<StringGraphType>>("accessRoles", resolve: context => context.Source.accessRoles);
            Field<ListGraphType<StringGraphType>>("darl", resolve: context => context.Source.darl);
            Field<ListGraphType<StringGraphType>>("implications", resolve: context => context.Source.implications);
        }

    }
}
