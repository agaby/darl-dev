/// <summary>
/// UpdateType.cs - Core module for the Darl.dev project.
/// </summary>

﻿using Darl.GraphQL.Models.Models;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class UpdateType : ObjectGraphType<Update>
    {
        public UpdateType()
        {
            Name = "update";
            Description = "A system wide update of some kind";
            Field(c => c.from).Description("The source of the update.");
            Field(c => c.to).Description("The destination of the update.");
            Field(c => c.updated).Description("The utc time of the update.");
        }
    }
}
