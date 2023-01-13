using Darl.Lineage;
using GraphQL.Types;

namespace Darl.GraphQL.Process.Blazor.Schemata
{
    public class LineageMatchTreeType : ObjectGraphType<LineageMatchTree>
    {
        public LineageMatchTreeType()
        {
            Name = "LineageMatchTree";
            Description = "A text recognition tree";
            Field(c => c.changed);
            Field<LineageMatchNodeType>("root").Resolve(context => context.Source.root);
        }
    }
}
