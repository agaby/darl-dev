using Darl.GraphQL.Models.Models;
using Darl.Thinkbase;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Schemata
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
