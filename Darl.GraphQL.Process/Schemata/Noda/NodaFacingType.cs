using Darl.GraphQL.Models.Models.Noda;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata.Noda
{
    public class NodaFacingType : ObjectGraphType<NodaFacing>
    {
        public NodaFacingType()
        {
            Name = "NodaFacingType";
            Description = "Noda Facing descriptor";
            Field(c => c.w);
            Field(c => c.x);
            Field(c => c.y);
            Field(c => c.z);
        }
    }
}
