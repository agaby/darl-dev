using Darl.GraphQL.Models.Models;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class AuthorizationUpdateType: InputObjectGraphType<Authorization>
    {
        public AuthorizationUpdateType()
        {
            Name = "authorizationUpdate";
            Field<NonNullGraphType<StringGraphType>>("name");
            Field<NonNullGraphType<StringGraphType>>("param1");
            Field<NonNullGraphType<StringGraphType>>("param2");
            Field<NonNullGraphType<StringGraphType>>("param3");
            Field<NonNullGraphType<StringGraphType>>("param4");
        }
    }
}
