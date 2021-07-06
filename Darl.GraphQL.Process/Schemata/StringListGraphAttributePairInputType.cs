using Darl.GraphQL.Models.Models;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class StringListGraphAttributePairInputType : InputObjectGraphType<StringListGraphAttributePair>
    {
        public StringListGraphAttributePairInputType()
        {
            Name = "stringListGraphAttributePairInput";
            Field<NonNullGraphType<StringGraphType>>("name");
            Field<ListGraphType<GraphAttributeInputType>>("value");
        }
    }
}
