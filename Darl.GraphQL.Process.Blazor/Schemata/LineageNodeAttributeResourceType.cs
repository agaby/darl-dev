using Darl.GraphQL.Process.Blazor.Models;
using GraphQL.Types;

namespace Darl.GraphQL.Process.Blazor.Schemata
{
    public class LineageNodeAttributeResourceType : ObjectGraphType<LineageNodeAttributeResources>
    {
        public LineageNodeAttributeResourceType()
        {
            Name = "lineageNodeAttributeResources";
            Description = "Resources needed to edit or create LineageNodeAttributes";
            Field(c => c.ruleSkeleton);
            Field(c => c.insertionPointText);
            Field<ListGraphType<StringGraphType>>("allRoles").Resolve(context => context.Source.AllRoles);
            Field<ListGraphType<StringGraphType>>("allRulesets").Resolve(context => context.Source.AllRulesets);
        }
    }
}
