using Darl.GraphQL.Process.Blazor.Models;
using GraphQL.Types;

namespace Darl.GraphQL.Process.Blazor.Schemata
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
