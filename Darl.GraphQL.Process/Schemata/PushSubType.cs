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
            Field(c => c.pushEndPoint);
            Field(c => c.pushAuth);
            Field(c => c.pushKey);
            Field(c => c.ipAddress);
        }
    }
}
