using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Darl.GraphQL.Ui.Playground
{

    public class GraphQLPlaygroundOptions
    {

        public PathString Path { get; set; } = "/ui/playground";

        /// <summary>
        /// The GraphQL EndPoint
        /// </summary>
        public PathString GraphQLEndPoint { get; set; } = "/graphql";

        /// <summary>
        /// The GraphQL Config
        /// </summary>
        public Dictionary<string, object>? GraphQLConfig { get; set; } = null;

        /// <summary>
        /// The GraphQL Playground Settings
        /// </summary>
        public Dictionary<string, object>? PlaygroundSettings { get; set; } = null;

    }

}
