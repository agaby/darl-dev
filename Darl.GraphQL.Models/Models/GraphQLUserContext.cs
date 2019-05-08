using GraphQL.Authorization.AspNetCore;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace Darl.GraphQL.Models.Models
{
    public class GraphQLUserContext : IProvideClaimsPrincipal
    {
        public ClaimsPrincipal User { get; set; }

    }
}
