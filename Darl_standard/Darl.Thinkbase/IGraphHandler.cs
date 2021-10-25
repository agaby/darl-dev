using Darl.Lineage.Bot;
using DarlCommon;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Darl.Thinkbase
{
    public enum GraphProcess { seek, discover };

    public interface IGraphHandler
    {

        Task<List<KnowledgeRecord>> Discover(string userId, string KnowledgeGraphName, string subjectId, List<string> lineages);
        Task<(List<InteractTestResponse>, DarlVar?)> GraphPass(string userId, string graphName, string subjectId, string targetId, List<string> paths, string completionLineage, List<DarlVar> values, DarlVar? pending, GraphProcess graphProces);
        Task<List<InteractTestResponse>> InterpretText(string userId, string graphName, string subjectId, DarlVar conversationData);
    }
}