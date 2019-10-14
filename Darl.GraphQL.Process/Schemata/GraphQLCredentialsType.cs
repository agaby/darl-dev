using DarlCommon;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class GraphQLCredentialsType : ObjectGraphType<GraphQLCredentials>
    {
        public GraphQLCredentialsType()
        {
            Name = "graphQLCredentials";
            Description = "Credentials for accessing a graphQL site authorized via a header";
            Field(c => c.header).Description("Authorization header");
            Field(c => c.url).Description("full url to the site.");
        }
    }
}
