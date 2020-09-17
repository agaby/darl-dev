using Darl.Thinkbase;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class DisplayConnectionInnerType : ObjectGraphType<DisplayConnection>
    {
        public DisplayConnectionInnerType()
        {
            Field<StringGraphType>("id", resolve: c => c.Source.id);
            Field<StringGraphType>("label", resolve: c => c.Source.name);
            Field<StringGraphType>("source", resolve: c => c.Source.source);
            Field<StringGraphType>("target", resolve: c => c.Source.target);
        }
    }
}