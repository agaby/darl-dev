using Darl.Thinkbase;
using GraphQL.Types;

namespace Darl.GraphQL.Process.Blazor.Schemata
{
    public class VRDisplayNodeType : ObjectGraphType<VRDisplayNode>
    {
        public VRDisplayNodeType()
        {
            Name = "vrDisplayNode";
            Description = "A simplified version of a knowledge graph node for VR display purposes";
            Field<StringGraphType>("id").Resolve(c => c.Source.id);
            Field<StringGraphType>("label").Resolve(c => c.Source.name);
            Field<StringGraphType>("lineage").Resolve(c => c.Source.lineage);
            Field<StringGraphType>("sublineage").Resolve(c => c.Source.subLineage);
            Field<StringGraphType>("externalId").Resolve(c => c.Source.externalId);
            Field<StringGraphType>("parent").Resolve(c => c.Source.parent);
            Field<StringGraphType>("compositelineage").Resolve(c => c.Source.compositeLineage);
            Field<ListGraphType<VRDisplayAttType>>("attributes").Resolve(c => c.Source.attributes);
        }
    }
}