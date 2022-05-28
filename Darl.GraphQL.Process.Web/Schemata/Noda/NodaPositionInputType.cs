using Darl.GraphQL.Models.Models.Noda;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Schemata.Noda
{
    public class NodaPositionInputType : InputObjectGraphType<NodaPosition>
    {
        public NodaPositionInputType()
        {
            Name = "NodaPosition";
            Description = "The cartesian coordinates of a location within Noda";
            Field(c => c.x);
            Field(c => c.y);
            Field(c => c.z);
        }
    }
}
