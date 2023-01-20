namespace Darl.GraphQL.Blazor.Client.Models
{
    public class DisplayModel
    {
        public List<DisplayObject> nodes { get; set; }  = new List<DisplayObject>();

        public List<DisplayConnection> edges { get; set; } = new List<DisplayConnection>();
    }
}
