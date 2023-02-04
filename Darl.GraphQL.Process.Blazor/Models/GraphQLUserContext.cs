using System.Security.Claims;

namespace Darl.GraphQL.Process.Blazor.Models
{
    public class GraphQLUserContext : Dictionary<string, object?>
    {
        public GraphQLUserContext(ClaimsPrincipal user)
        {
            User = user;
        }

        public ClaimsPrincipal User { get; set; }
    }
}
