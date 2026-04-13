/// <summary>
/// PlaygroundExtensions.cs - Core module for the Darl.dev project.
/// </summary>

using Microsoft.AspNetCore.Builder;

namespace Darl.GraphQL.Container.Ui.Playground
{

    public static class PlaygroundExtensions
    {

        public static IApplicationBuilder UseGraphQLPlayground(this IApplicationBuilder app, GraphQLPlaygroundOptions? options = null)
        {
            return app.UseMiddleware<PlaygroundMiddleware>(options ?? new GraphQLPlaygroundOptions());
        }
    }
}
