/// <summary>
/// StringDoublePairInputType.cs - Core module for the Darl.dev project.
/// </summary>

﻿using Darl.GraphQL.Models.Models;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class StringDoublePairInputType : InputObjectGraphType<StringDoublePair>
    {
        public StringDoublePairInputType()
        {
            Name = "stringDoublePairInput";
            Field<NonNullGraphType<StringGraphType>>("name");
            Field<NonNullGraphType<FloatGraphType>>("value");
        }
    }
}
