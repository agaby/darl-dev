using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace Darl.GraphQL.Models.Middleware
{
    public class GraphQLSettings
    {
        public PathString Path { get; set; } = "/graphql";
        public Func<HttpContext, IDictionary<string, object>> BuildUserContext { get; set; }
        public bool EnableMetrics { get; set; }
        public bool ExposeExceptions { get; set; }
    }
}
