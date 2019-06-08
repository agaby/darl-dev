using GraphQL.Server.Authorization.AspNetCore;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace Darl.GraphQL.Models.Models
{
    public class GraphQLUserContext : Dictionary<string, object>
    {
        public ClaimsPrincipal User { get; set; }

    }
}
