using Darl.Thinkbase;
using GraphQL.Types;

namespace Darl.GraphQL.Process.Blazor.Schemata
{
    public class VRDisplayModelType : ObjectGraphType<VRDisplayModel>
    {
        public VRDisplayModelType()
        {
            Name = "vrDisplayModel";
            Description = "A simplified version of a knowledge graph for VR display purposes";
            Field<ListGraphType<VRDisplayNodeType>>("nodes").Resolve(context => context.Source.nodes);
            Field<ListGraphType<VRDisplayLinkType>>("links").Resolve(context => context.Source.links);
        }
    }
}
