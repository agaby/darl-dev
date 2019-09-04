using Darl.GraphQL.Models.Schemata;
using DarlCommon;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class DaslStateInputType : InputObjectGraphType<DaslState>
    {

        public DaslStateInputType()
        {
            Name = "daslStateInput";
            Description = "The state of a system at a particular time";
            Field<DateTimeGraphType>("timeStamp","The point in time of this state");
            Field<ListGraphType<DarlVarInputType>>("values", "The set of values changed at this state", resolve: context => context.Source.values);

        }
    }
}
