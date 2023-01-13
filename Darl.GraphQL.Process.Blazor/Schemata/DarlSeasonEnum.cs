using Darl.GraphQL.Process.Blazor.Models;
using GraphQL.Types;

namespace Darl.GraphQL.Process.Blazor.Schemata
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
