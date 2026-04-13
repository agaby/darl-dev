/// <summary>
/// StringStringPairType.cs - Core module for the Darl.dev project.
/// </summary>

﻿using Darl.Thinkbase;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class StringStringPairType : ObjectGraphType<StringStringPair>
    {
        public StringStringPairType()
        {
            Name = "StringStringPair";
            Description = "a name value pair where the value is a string.";
            Field(c => c.Name);
            Field(c => c.Value);
        }
    }
}
