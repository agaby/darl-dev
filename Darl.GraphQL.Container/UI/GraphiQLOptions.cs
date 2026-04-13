/// </summary>

using Microsoft.AspNetCore.Http;

namespace Darl.GraphQL.Container.Ui.GraphiQL
{
    /// The settings of the <see cref="GraphiQLMiddleware"/>
    /// </summary>
    public class GraphiQLOptions
    {
        /// The GraphiQL Endpoint to listen
        /// </summary>
        public PathString GraphiQLPath { get; set; } = "/graphiql";

        /// The GraphQL EndPoint
        /// </summary>
        public PathString GraphQLEndPoint { get; set; } = "/graphql";
    }
}
