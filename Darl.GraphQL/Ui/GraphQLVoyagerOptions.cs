using Microsoft.AspNetCore.Http;

namespace Darl.GraphQL.Ui.Voyager
{
    public class GraphQLVoyagerOptions
    {
        public PathString Path { get; set; } = "/ui/voyager";

        /// <summary>
        /// The GraphQL EndPoint
        /// </summary>
        public PathString GraphQLEndPoint { get; set; } = "/graphql";
    }
}
