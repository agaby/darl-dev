using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata.Noda
{
    public class NodaNodeShapeEnum : EnumerationGraphType
    {
        public NodaNodeShapeEnum()
        {
            Name = "NodaNodeShape";
            AddValue("Ball", "Ball", 0);
            AddValue("Box", "Box", 1);
        }
    }
}
