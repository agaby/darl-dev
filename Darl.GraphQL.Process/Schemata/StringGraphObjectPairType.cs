using Darl.GraphQL.Models.Models;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class StringGraphObjectPairType : ObjectGraphType<StringGraphObjectPair>
    {
        public StringGraphObjectPairType()
        {
            Name = "StringGraphObjectPair";
            Description = "a name value pair where the value is a GraphObject.";
            Field(c => c.Name);
            Field<GraphObjectType>("value", resolve: c => c.Source.Value);
        }
    }
}
