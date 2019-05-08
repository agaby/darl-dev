using Darl.GraphQL.Models.Models;
using GraphQL.Authorization.AspNetCore;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class DefaultType : ObjectGraphType<Default>
    {
        public DefaultType()
        {
            Name = "Default";
            this.AuthorizeWith("AdminPolicy");

            Description = "Name value pairs used to configure the system";
            Field(c => c.Name);
            Field(c => c.Value);
        }
    }
}
