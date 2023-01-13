using GraphQL.Types;
using static Darl.Thinkbase.IGraphModel;

namespace Darl.GraphQL.Process.Blazor.Schemata
{
    public class DateDisplayEnum : EnumerationGraphType<DateDisplay>
    {
        public DateDisplayEnum()
        {
            Name = "DateDisplay";
            Description = "Determines if displayed dates are modern or historic in form.";
        }
    }
}
