using Darl.GraphQL.Models.Models;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class StringDarlVarPairType : ObjectGraphType<StringDarlVarPair>
    {
        public StringDarlVarPairType()
        {
            Name = "StringStringPair";
            Description = "a name value pair where the value is a DarlVar.";
            Field(c => c.Name);
            Field<DarlVarType>("value", resolve: c => c.Source.Value);
        }
    }
}
