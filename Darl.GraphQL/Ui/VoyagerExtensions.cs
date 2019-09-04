using Microsoft.AspNetCore.Builder;

namespace Darl.GraphQL.Ui.Voyager
{
    public static class VoyagerExtensions
    {
        public static IApplicationBuilder UseGraphQLVoyager(this IApplicationBuilder app, GraphQLVoyagerOptions options = null)
        {
            return app.UseMiddleware<VoyagerMiddleware>(options ?? new GraphQLVoyagerOptions());
        }
    }
}
