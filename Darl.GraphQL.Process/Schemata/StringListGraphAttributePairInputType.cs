using Darl.GraphQL.Models.Models;
using Darl.Thinkbase;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class StringListGraphAttributeInputPairInputType : InputObjectGraphType<StringListGraphAttributeInputPair>
    {
        public StringListGraphAttributeInputPairInputType()
        {
            Name = "stringListGraphAttributeInputPairInput";
            Field<NonNullGraphType<StringGraphType>>("name");
            Field< NonNullGraphType<ListGraphType<GraphAttributeInputType>>>("value");
        }
    }
}
