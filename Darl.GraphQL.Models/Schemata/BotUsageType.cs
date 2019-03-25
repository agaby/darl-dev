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
            Name = "BotUsage";
            Description = "A day of bot usage and the count of interactions.";
            Field(c => c.Date);
            Field(c => c.Count);
        }
    }
}
