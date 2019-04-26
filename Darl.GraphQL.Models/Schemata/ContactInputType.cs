using Darl.GraphQL.Models.Models;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class ContactInputType : InputObjectGraphType
    {
        public ContactInputType()
        {
            Name = "ContactInput";
            Description = "A contact that has requested to be informed about DARL.ai";
            Field<StringGraphType>("company");
            Field<StringGraphType>("country");
            Field<StringGraphType>("firstName");
            Field<StringGraphType>("lastName");
            Field<NonNullGraphType<StringGraphType>>("email");
            Field<StringGraphType>("phone");
            Field<StringGraphType>("title");
            Field<StringGraphType>("source");
            Field<StringGraphType>("notes");
            Field<StringGraphType>("sector");
            Field<BooleanGraphType>("introSent");
        }

    }
}
