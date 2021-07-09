using GraphQL.Validation.Complexity;

namespace Darl.GraphQL.Models.Middleware
{
    public class GraphQLOptions
    {
        public ComplexityConfiguration ComplexityConfiguration { get; set; }

        public bool EnableMetrics { get; set; } = true;

        public bool ExposeExceptions { get; set; } = true;

        public bool SetFieldMiddleware { get; set; } = true;
    }
}
