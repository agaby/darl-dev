using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Darl.GraphQL.Models.Models;
using Darl.Lineage;
using System.Linq;

namespace Darl.GraphQL.Models.Services
{
    class LineageMatchNodePairSevice : ILineageMatchNodePairService
    {
        public async Task<List<LineageMatchNodePair>> GetChildrenAsPairs(SortedList<string, LineageMatchNode> children)
        {
            var list = new List<LineageMatchNodePair>();
            foreach(var k in  children.Keys)
            {
                list.Add(new LineageMatchNodePair(k, children[k]));
            }
            return list;
        }
    }
}
