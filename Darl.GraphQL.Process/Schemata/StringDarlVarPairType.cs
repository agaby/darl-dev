/// </summary>

﻿using Darl.GraphQL.Models.Models;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class StringDarlVarPairType : ObjectGraphType<StringDarlVarPair>
    {
        public StringDarlVarPairType()
        {
            Name = "StringDarlVarPair";
            Description = "a name value pair where the value is a DarlVar.";
            Field(c => c.Name);
            Field<DarlVarType>("value", resolve: c => c.Source.Value);
        }
    }
}
