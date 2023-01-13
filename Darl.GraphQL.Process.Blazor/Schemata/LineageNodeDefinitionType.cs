using Darl.GraphQL.Process.Blazor.Models;
using GraphQL.Types;

namespace Darl.GraphQL.Process.Blazor.Schemata
{
    public class LineageNodeDefinitionType : ObjectGraphType<LineageNodeDefinition>
    {
        public LineageNodeDefinitionType()
        {
            Name = "lineageNodeDefinition";
            Description = "data required to build an editable tree of the text engine contents";
            Field(c => c.children);
            Field(c => c.id, true);
            Field(c => c.text, true);
            Field(c => c.icon, true);
            Field(c => c.type, true);
            Field<LineageNodeAttributeType>("attributes").Resolve(c => c.Source.attributes);
        }
    }
}
