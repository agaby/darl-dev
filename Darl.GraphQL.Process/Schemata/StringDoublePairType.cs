/// </summary>

﻿using Darl.GraphQL.Models.Models;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class StringDoublePairType : ObjectGraphType<StringDoublePair>
    {
        public StringDoublePairType()
        {
            Name = "StringDoublePair";
            Description = "a name value pair where the value is a double.";
            Field(c => c.name);
            Field(c => c.value);
        }
    }
}
