/// <summary>
/// GraphQLUserContext.cs - Core module for the Darl.dev project.
/// </summary>

using System.Collections.Generic;
using System.Security.Claims;

namespace Darl.GraphQL.Models.Middleware
{
    public class GraphQLUserContext : Dictionary<string, object>
    {
        public ClaimsPrincipal User { get; set; }
    }
}
