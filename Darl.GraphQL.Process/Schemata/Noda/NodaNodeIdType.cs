using Darl.GraphQL.Models.Models.Noda;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata.Noda
{
    class NodaNodeIdType : ObjectGraphType<NodaNodeId>
    {
        public NodaNodeIdType()
        {
            Name = "NodaNodeIdType";
            Description = "Noda Node Id descriptor";
            Field(c => c.id);
            Field(c => c.Uuid);
        }
    }
}
