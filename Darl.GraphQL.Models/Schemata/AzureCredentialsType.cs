using Darl.Connectivity.Models;
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
            Field(c => c.AzureAPIKey);
        }
    }
}
