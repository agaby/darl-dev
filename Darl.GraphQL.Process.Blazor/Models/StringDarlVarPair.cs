using DarlCommon;

namespace Darl.GraphQL.Process.Blazor.Models
{
    public class StringDarlVarPair
    {
        public string Name { get; set; } = string.Empty;

        public DarlVar Value { get; set; } = new DarlVar();
    }
}
