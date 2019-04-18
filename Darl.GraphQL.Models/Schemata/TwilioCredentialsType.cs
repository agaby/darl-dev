using Darl.GraphQL.Models.Models;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class TwilioCredentialsType : ObjectGraphType<TwilioCredentials>
    {
        public TwilioCredentialsType()
        {
            Name = "TwilioCredentials";
            Description = "Credentials to access Twilio.";
            Field(c => c.SMSAccountFrom);
            Field(c => c.SMSAccountIdentification);
            Field(c => c.SMSAccountPassword);
        }
    }
}
