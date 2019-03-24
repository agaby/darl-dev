using Darl.Connectivity.Models;
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
            Field(c => c.SMSAccountFrom);
            Field(c => c.SMSAccountIdentification);
            Field(c => c.SMSAccountPassword);
        }
    }
}
