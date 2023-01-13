using Darl.GraphQL.Process.Blazor.Models;
using GraphQL.Types;

namespace Darl.GraphQL.Process.Blazor.Schemata
{
    public class StringGraphConnectionPairType : ObjectGraphType<StringGraphConnectionPair>
    {
        public StringGraphConnectionPairType()
        {
            Name = "StringGraphConnectionPair";
            Description = "a name value pair where the value is a GraphConnection.";
            Field(c => c.Name);
            Field<GraphConnectionType>("value").Resolve(c => c.Source.Value);
        }
    }
}
