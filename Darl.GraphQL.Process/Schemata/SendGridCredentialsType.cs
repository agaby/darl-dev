using Darl.GraphQL.Models.Models;
using DarlCommon;
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
            Name = "SendGridCredentials";
            Description = "Credentials to access SendGrid.";
            Field(c => c.SendGridAPIKey);
        }
    }
}
