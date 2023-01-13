namespace Darl.GraphQL.Blazor.Server.Helpers
{
    public class GraphQLSettings
    {        
        public bool EnableMetrics { get; set; }
        public bool ExposeExceptions { get; internal set; } = true;
    }
}
