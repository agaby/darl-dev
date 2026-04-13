/// </summary>

﻿using Darl.GraphQL.Models.Models;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class PushSubType : ObjectGraphType<PushSub>
    {
        public PushSubType()
        {
            Name = "pushSubType";
            Description = "The data for a push subscription.";
            Field(c => c.pushEndPoint);
            Field(c => c.pushAuth);
            Field(c => c.pushKey);
            Field(c => c.ipAddress);
            Field(c => c.longitude);
            Field(c => c.latitude);
            Field(c => c.created, true);
        }
    }
}
