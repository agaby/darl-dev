using Darl.Connectivity.Models;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class ZendeskCredentialsType : ObjectGraphType<ZendeskCredentials>
    {
        public ZendeskCredentialsType()
        {
            Field(c => c.ZendeskApiKey);
            Field(c => c.ZendeskURL);
            Field(c => c.ZendeskUser);
        }
    }
}
