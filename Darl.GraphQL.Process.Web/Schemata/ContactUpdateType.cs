/// <summary>
/// ContactUpdateType.cs - Core module for the Darl.dev project.
/// </summary>

﻿using Darl.GraphQL.Models.Models;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class ContactUpdateType : InputObjectGraphType<ContactUpdate>
    {
        public ContactUpdateType()
        {
            Name = "ContactUpdate";
            Description = "A contact that has requested to be informed about DARL.ai";
            Field<StringGraphType>("company");
            Field<StringGraphType>("firstName");
            Field<StringGraphType>("lastName");
            Field<NonNullGraphType<StringGraphType>>("email");
            Field<StringGraphType>("phone");
            Field<StringGraphType>("title");
            Field<StringGraphType>("source");
            Field<StringGraphType>("notes");
            Field<StringGraphType>("sector");
            Field<StringGraphType>("country");
            Field<BooleanGraphType>("introSent");
            Field<StringGraphType>("id");
        }
    }
}
