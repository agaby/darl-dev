using Darl.GraphQL.Models.Models;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class LineageNodeAttributeResourceType : ObjectGraphType<LineageNodeAttributeResources>
    {
        public LineageNodeAttributeResourceType()
        {
            Name = "lineageNodeAttributeResources";
            Description = "Resources needed to edit or create LineageNodeAttributes";
            Field(c => c.ruleSkeleton);
            Field(c => c.insertionPointText);
            Field<ListGraphType<StringGraphType>>("allRoles", resolve: context => context.Source.AllRoles);
            Field<ListGraphType<StringGraphType>>("allRulesets", resolve: context => context.Source.AllRulesets);
        }
    }
}
