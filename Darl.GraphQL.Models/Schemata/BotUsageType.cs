using Darl.GraphQL.Models.Models;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class BotUsageType : ObjectGraphType<BotUsage>
    {
        public BotUsageType()
        {
            Field(c => c.Date);
            Field(c => c.Count);
        }
    }
}
