using Darl.GraphQL.Models.Models;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class DarlSeasonEnum : EnumerationGraphType<DarlSeason>
    {
        public DarlSeasonEnum()
        {
            Name = "season";
            Description = "Season for use with BC dates in DarlTime";
        }
    }
}
