using Darl.Common;
using Darl.GraphQL.Models.Models;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class DarlTimeInputType : InputObjectGraphType<DarlTimeInput>
    {
        public DarlTimeInputType()
        {
            Name = "DarlTimeInput";
            Description = "A time representation that includes BC as well as AD times";
            Field(c => c.raw, true).Description("A value representing the number of seconds since 1 AD. Negative numbers are seconds before this date.");
            Field(c => c.dateTimeOffset, true).Description("A standardized time value not valid for BC.");
            Field(c => c.precision, true).Description("The precision of the raw value in seconds");
            Field(c => c.year, true).Description("The year of the event, useful for historic dates. Negative numbers are BC."); 
            Field<DarlSeasonEnum>("season","Season, useful as a refinement for historic dates.", resolve: c => c.Source.season);
        }
    }
}
