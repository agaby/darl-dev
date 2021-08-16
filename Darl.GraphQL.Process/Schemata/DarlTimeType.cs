using Darl.Common;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class DarlTimeType : ObjectGraphType<DarlTime>
    {
        public DarlTimeType()
        {
            Name = "DarlTime";
            Description = "A time representation that includes BC as well as AD times";
            Field(c => c.raw, true);
            Field(c => c.dateTime, true);
            Field(c => c.dateTimeOffset, true);
            Field(c => c.precision, true);
            Field(c => c.y, true);
        }
    }
}
