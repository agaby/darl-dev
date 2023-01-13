using Darl.GraphQL.Process.Blazor.Models;
using GraphQL.Types;

namespace Darl.GraphQL.Process.Blazor.Schemata
{
    public class StringListGraphAttributePairType : ObjectGraphType<StringListGraphAttributePair>
    {
        public StringListGraphAttributePairType()
        {
            Name = "StringListGraphAttributePair";
            Description = "a name value pair where the value is a list of GraphAttrubutes.";
            Field(c => c.Name);
            Field<ListGraphType<GraphAttributeType>>("value").Resolve(c => c.Source.Value);
        }


    }
}
