/// </summary>

using Darl.GraphQL.Container.Ui.GraphiQL;

namespace Microsoft.AspNetCore.Builder
{

    /// Extension methods for <see cref="GraphiQLMiddleware"/>
    /// </summary>
    public static class GraphiQLMiddlewareExtensions
    {

        /// Enables a GraphiQLServer using the specified settings
        /// </summary>
        /// <param name="applicationBuilder"></param>
        /// <param name="settings">The settings of the Middleware</param>
        /// <returns></returns>
        public static IApplicationBuilder UseGraphiQLServer(this IApplicationBuilder applicationBuilder, GraphiQLOptions? settings = null)
        {
            return applicationBuilder.UseMiddleware<GraphiQLMiddleware>(settings ?? new GraphiQLOptions());
        }

    }

}
