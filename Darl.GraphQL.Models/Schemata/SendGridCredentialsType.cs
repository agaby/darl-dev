using Darl.Connectivity.Models;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class SendGridCredentialsType : ObjectGraphType<SendGridCredentials>
    {
        public SendGridCredentialsType()
        {
            Field(c => c.SendGridAPIKey);
        }
    }
}
