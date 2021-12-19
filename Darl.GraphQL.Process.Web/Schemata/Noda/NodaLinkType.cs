using Darl.GraphQL.Models.Models.Noda;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata.Noda
{
    public class NodaLinkType : ObjectGraphType<NodaLink>
    {
        public NodaLinkType()
        {
            Name = "NodaLinkType";
            Description = "Noda Link descriptor";
            Field(c => c.kind);
            Field(c => c.folded);
            Field(c => c.size);
            Field<ListGraphType<NodaPropertyType>>("properties", resolve: context => context.Source.properties);
            Field<NodaLinkShapeEnum>("shape", resolve: context => context.Source.shape);
            Field<NodaNodeIdType>("fromNode", resolve: context => context.Source.fromNode);
            Field<NodaNodeIdType>("toNode", resolve: context => context.Source.toNode);
            Field<NodaToneType>("tone", resolve: context => context.Source.tone);
            Field(c => c.title);
            Field(c => c.uuid);
        }
    }
}
