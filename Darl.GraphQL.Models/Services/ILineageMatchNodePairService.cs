using Darl.GraphQL.Models.Models;
using Darl.Lineage;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Services
{
    public interface ILineageMatchNodePairService
    {
        Task<List<LineageMatchNodePair>> GetChildrenAsPairs(SortedList<string, LineageMatchNode> children);
    }
}
