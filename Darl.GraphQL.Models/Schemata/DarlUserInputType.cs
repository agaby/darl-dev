using Darl.GraphQL.Models.Models;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class DarlUserInputType : InputObjectGraphType<DarlUserInput>
    {
        public DarlUserInputType()
        {
            Name = "darlUserInput";
            Field<NonNullGraphType<StringGraphType>>("company");
            Field<NonNullGraphType<StringGraphType>>("firstName");
            Field<NonNullGraphType<StringGraphType>>("lastName");
            Field<NonNullGraphType<StringGraphType>>("email");
            Field<NonNullGraphType<StringGraphType>>("phone");
            Field<NonNullGraphType<StringGraphType>>("title");
            Field<NonNullGraphType<StringGraphType>>("source");
            Field<NonNullGraphType<StringGraphType>>("notes");
            Field<NonNullGraphType<StringGraphType>>("sector");
            Field<NonNullGraphType<StringGraphType>>("introSent");
            Field<NonNullGraphType<StringGraphType>>("id");
        }
    }
}
