using Darl.GraphQL.Models.Models;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class StringStringPairInputType : InputObjectGraphType<StringStringPair>
    {
        public StringStringPairInputType()
        {
            Name = "stringStrinpPairInput";
            Field<NonNullGraphType<StringGraphType>>("name");
            Field<NonNullGraphType<StringGraphType>>("value");
        }
    }
}
