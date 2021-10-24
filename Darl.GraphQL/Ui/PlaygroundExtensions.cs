using Microsoft.AspNetCore.Builder;

namespace Darl.GraphQL.Ui.Playground
{

    public static class PlaygroundExtensions
    {

        public static IApplicationBuilder UseGraphQLPlayground(this IApplicationBuilder app, GraphQLPlaygroundOptions? options = null)
        {
            return app.UseMiddleware<PlaygroundMiddleware>(options ?? new GraphQLPlaygroundOptions());
        }
    }
}
