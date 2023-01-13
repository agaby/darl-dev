using Darl.GraphQL.Process.Blazor.Models;
using GraphQL.Types;

namespace Darl.GraphQL.Process.Blazor.Schemata
{
    public class StringGraphObjectPairType : ObjectGraphType<StringGraphObjectPair>
    {
        public StringGraphObjectPairType()
        {
            Name = "StringGraphObjectPair";
            Description = "a name value pair where the value is a GraphObject.";
            Field(c => c.Name);
            Field<GraphObjectType>("value").Resolve(c => c.Source.Value);
        }
    }
}
