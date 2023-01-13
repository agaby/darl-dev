using Darl.Thinkbase;
using GraphQL.Types;

namespace Darl.GraphQL.Process.Blazor.Schemata
{
    public class VRDisplayLinkType : ObjectGraphType<VRDisplayLink>
    {
        public VRDisplayLinkType()
        {
            Name = "vrDisplayLink";
            Description = "A simplified version of a knowledge graph edge for VR display purposes";
            Field<StringGraphType>("id").Resolve(c => c.Source.id);
            Field<StringGraphType>("label").Resolve(c => c.Source.name);
            Field<StringGraphType>("source").Resolve(c => c.Source.source);
            Field<StringGraphType>("target").Resolve(c => c.Source.target);
        }
    }
}