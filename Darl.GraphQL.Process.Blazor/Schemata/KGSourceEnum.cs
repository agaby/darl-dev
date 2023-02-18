using Darl.GraphQL.Process.Blazor.Models;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThinkBase.ComponentLibrary.Models;

namespace Darl.GraphQL.Process.Blazor.Schemata
{
    public class KGSourceEnum : EnumerationGraphType<KGSource>
    {
        public KGSourceEnum()
        {
            Name = "KGSource";
            Description = "Shows if this KG is available to the team or just an individual.";
        }
    }
}