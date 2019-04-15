using Darl.GraphQL.Models.Models;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class BotOutputFormatUpdateType : InputObjectGraphType<BotOutputFormatUpdate>
    {
        public BotOutputFormatUpdateType()
        {
            Name = "botOutputFormatUpdate";
            Field<NonNullGraphType<StringGraphType>>("valueFormat");
            Field<NonNullGraphType<DisplayTypeEnum>>("displayType");

        }
    }
}
