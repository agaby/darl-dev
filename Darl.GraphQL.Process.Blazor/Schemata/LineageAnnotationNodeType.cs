using Darl.Lineage;
using GraphQL.Types;

namespace Darl.GraphQL.Process.Blazor.Schemata
{
    public class LineageAnnotationNodeType : ObjectGraphType<LineageAnnotationNode>
    {
        public LineageAnnotationNodeType()
        {
            Name = "LineageAnnotationNode";
            Description = "Annotation to define a node as terminal and define the actions associated with it.";
            Field<ListGraphType<StringGraphType>>("accessRoles").Resolve(context => context.Source.accessRoles);
            Field<ListGraphType<StringGraphType>>("darl").Resolve(context => context.Source.darl);
            Field<ListGraphType<StringGraphType>>("implications").Resolve(context => context.Source.implications);
        }

    }
}
