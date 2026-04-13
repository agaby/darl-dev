/// </summary>

using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Darl.GraphQL.Container.Ui.Playground
{

    public class GraphQLPlaygroundOptions
    {

        public PathString Path { get; set; } = "/ui/playground";

        /// The GraphQL EndPoint
        /// </summary>
        public PathString GraphQLEndPoint { get; set; } = "/graphql";

        /// The GraphQL Config
        /// </summary>
        public Dictionary<string, object>? GraphQLConfig { get; set; } = null;

        /// The GraphQL Playground Settings
        /// </summary>
        public Dictionary<string, object>? PlaygroundSettings { get; set; } = null;

    }

}
