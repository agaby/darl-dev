using Darl.GraphQL.Models.Models.Noda;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata.Noda
{
    public class NodaToneType : ObjectGraphType<NodaTone>
    {
        public NodaToneType()
        {
            Name = "NodaToneType";
            Description = "Noda color descriptor";
            Field(c => c.a);
            Field(c => c.r);
            Field(c => c.g);
            Field(c => c.b);
        }
    }
}
