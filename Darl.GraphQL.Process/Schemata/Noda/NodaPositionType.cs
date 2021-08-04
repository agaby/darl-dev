using Darl.GraphQL.Models.Models.Noda;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata.Noda
{
    class NodaPositionType : ObjectGraphType<NodaPosition>
    {
        public NodaPositionType()
        {
            Name = "NodaPositionType";
            Description = "Noda position descriptor";
            Field(c => c.x);
            Field(c => c.y);
            Field(c => c.z);
        }
    }
}
