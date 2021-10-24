using Darl.GraphQL.Models.Models;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class StringListGraphAttributePairType : ObjectGraphType<StringListGraphAttributePair>
    {
        public StringListGraphAttributePairType()
        {
            Name = "StringListGraphAttributePair";
            Description = "a name value pair where the value is a list of GraphAttrubutes.";
            Field(c => c.Name);
            Field<ListGraphType<GraphAttributeType>>("value", resolve: c => c.Source.Value);
        }


    }
}
