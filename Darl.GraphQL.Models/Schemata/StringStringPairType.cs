using Darl.GraphQL.Models.Models;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

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
