using Darl.GraphQL.Models.Models;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class AuthorizationType : ObjectGraphType<Authorization>
    {
        public AuthorizationType()
        {
            Name = "authorization";
            Description = "settings for Oauth type authorizations for the bot";
            Field(c => c.name);
            Field(c => c.param1);
            Field(c => c.param2);
            Field(c => c.param3);
            Field(c => c.param4);
        }
    }
}
