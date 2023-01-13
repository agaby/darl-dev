using Darl.Thinkbase;
using GraphQL.Types;

namespace Darl.GraphQL.Process.Blazor.Schemata
{
    public class StringStringPairInputType : InputObjectGraphType<StringStringPair>
    {
        public StringStringPairInputType()
        {
            Name = "stringStringPairInput";
            Field<NonNullGraphType<StringGraphType>>("name");
            Field<NonNullGraphType<StringGraphType>>("value");
        }
    }
}
