/// <summary>
/// StringGraphConnectionPairType.cs - Core module for the Darl.dev project.
/// </summary>

﻿using Darl.GraphQL.Models.Models;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class StringGraphConnectionPairType : ObjectGraphType<StringGraphConnectionPair>
    {
        public StringGraphConnectionPairType()
        {
            Name = "StringGraphConnectionPair";
            Description = "a name value pair where the value is a GraphConnection.";
            Field(c => c.Name);
            Field<GraphConnectionType>("value", resolve: c => c.Source.Value);
        }
    }
}
