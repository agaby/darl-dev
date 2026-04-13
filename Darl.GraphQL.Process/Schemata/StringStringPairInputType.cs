/// <summary>
/// </summary>

﻿using Darl.Thinkbase;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
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
