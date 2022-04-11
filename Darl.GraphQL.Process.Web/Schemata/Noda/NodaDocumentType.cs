using Darl.GraphQL.Models.Models.Noda;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata.Noda
{
    public class NodaDocumentType : ObjectGraphType<NodaDocument>
    {
        public NodaDocumentType()
        {
            Name = "NodaDocumentType";
            Description = "Noda Document descriptor";
            Field(c => c.name);
            Field(c => c.format);
            Field(c => c.description);
            Field(c => c.initialText);
            Field<ListGraphType<NodaNodeType>>("metaNodes", resolve: context => context.Source.metaNodes);
            Field<ListGraphType<NodaNodeType>>("nodes", resolve: context => context.Source.nodes);
            Field<ListGraphType<NodaLinkType>>("metaLinks", resolve: context => context.Source.metaLinks);
            Field<ListGraphType<NodaLinkType>>("links", resolve: context => context.Source.links);
        }
    }
}
