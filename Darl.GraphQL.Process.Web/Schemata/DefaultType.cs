/// <summary>
/// DefaultType.cs - Core module for the Darl.dev project.
/// </summary>

﻿using Darl.GraphQL.Models.Models;
using GraphQL;
using GraphQL.Types;

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
