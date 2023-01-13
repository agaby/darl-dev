using Darl.GraphQL.Process.Blazor.Models;
using GraphQL.Types;

namespace Darl.GraphQL.Process.Blazor.Schemata
{
    public class StringDarlVarPairType : ObjectGraphType<StringDarlVarPair>
    {
        public StringDarlVarPairType()
        {
            Name = "StringDarlVarPair";
            Description = "a name value pair where the value is a DarlVar.";
            Field(c => c.Name);
            Field<DarlVarType>("value").Resolve(c => c.Source.Value);
        }
    }
}
