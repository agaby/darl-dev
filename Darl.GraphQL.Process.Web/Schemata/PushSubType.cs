using Darl.GraphQL.Models.Models;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
