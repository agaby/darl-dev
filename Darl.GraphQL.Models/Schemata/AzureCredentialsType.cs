using Darl.GraphQL.Models.Models;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class AzureCredentialsType  : ObjectGraphType<AzureCredentials>
    {
        public AzureCredentialsType()
        {
            Name = "AzureCredentials";
            Description = "The user's credentials for accessing Azure storage.";
            Field(c => c.AzureAPIKey);
        }
    }
}
