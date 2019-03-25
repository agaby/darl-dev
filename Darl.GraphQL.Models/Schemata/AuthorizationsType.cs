using Darl.Connectivity.Models;
using Darl.GraphQL.Models.Models;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class AuthorizationsType : ObjectGraphType<TableAuthorizations>
    {
        public AuthorizationsType()
        {
            Name = "Authorization";
            Description = "An authorization type granting access to some responses.";
            Field(c => c.name);
            Field<StringGraphType>("id", resolve: context => context.Source.PartitionKey);
        }
    }
}
