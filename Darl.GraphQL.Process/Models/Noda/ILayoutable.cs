using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Darl.GraphQL.Process.Web.Models.Noda
{
    public interface ILayoutable
    {
        ILayoutLink? GetEdge(string fromNode, string toNode);
        ILayoutNode? GetNode(string uuid);
        List<ILayoutNode> GetNodes();
        List<ILayoutLink> GetLinks();
        void Init();
    }
}
