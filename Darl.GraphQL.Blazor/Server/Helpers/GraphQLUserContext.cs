using System.Security.Claims;

namespace Darl.GraphQL.Blazor.Server.Helpers
{
    public class GraphQLUserContext : Dictionary<string, object>
    {
        public ClaimsPrincipal User { get; set; }
    }
}
