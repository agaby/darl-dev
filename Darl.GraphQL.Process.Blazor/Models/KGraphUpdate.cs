namespace Darl.GraphQL.Process.Blazor.Models
{
    public class KGraphUpdate
    {
        public string Name { get; set; }
        public string userId { get; set; }
        public bool? ReadOnly { get; set; } = false;
        public bool? hidden { get; set; } = false;
    }
}
