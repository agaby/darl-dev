using Darl.Thinkbase;

namespace Darl.GraphQL.Process.Blazor.Models
{
    public class StringListGraphAttributePair
    {
        public string Name { get; set; }

        public List<GraphAttribute> Value { get; set; }
    }
}
