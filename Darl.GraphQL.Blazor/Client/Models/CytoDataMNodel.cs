namespace Darl.GraphQL.Blazor.Client.Models
{
    public class CytoDataModel
    {
        public List<CytoDataNodeElement> nodes { get; set; } = new List<CytoDataNodeElement>();
        public List<CytoDataEdgeElement> edges { get; set; } = new List<CytoDataEdgeElement>();
    }
}
