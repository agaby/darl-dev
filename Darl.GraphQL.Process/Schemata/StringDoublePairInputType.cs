using Darl.GraphQL.Models.Models;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class StringDoublePairInputType : InputObjectGraphType<StringDoublePair>
    {
        public StringDoublePairInputType()
        {
            Name = "stringDoublePairInput";
            Field<NonNullGraphType<StringGraphType>>("name");
            Field<NonNullGraphType<FloatGraphType>>("value");
        }
    }
}
