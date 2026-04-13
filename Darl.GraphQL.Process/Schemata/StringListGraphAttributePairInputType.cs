/// <summary>
/// </summary>

﻿using Darl.Thinkbase;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class StringListGraphAttributeInputPairInputType : InputObjectGraphType<StringListGraphAttributeInputPair>
    {
        public StringListGraphAttributeInputPairInputType()
        {
            Name = "stringListGraphAttributeInputPairInput";
            Field<NonNullGraphType<StringGraphType>>("name");
            Field<NonNullGraphType<ListGraphType<GraphAttributeInputType>>>("value");
        }
    }
}
