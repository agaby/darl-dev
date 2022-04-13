using Darl.GraphQL.Models.Models.Noda;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Darl.GraphQL.Process.Web.Models.Noda
{
    public interface ILayoutNode
    {
        string uuid { get; set; }
        NodaPosition position { get; set; }
    }
}
