using Darl.Thinkbase;
using GraphQL.Types;

namespace Darl.GraphQL.Process.Blazor.Schemata
{
    public class ThinkBaseProcessType : EnumerationGraphType<GraphProcess>
    {
        public ThinkBaseProcessType()
        {
            Name = "process";
            Description = "The kind of process to perform.";
        }
    }
}
