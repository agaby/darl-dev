using Darl.Common;
using Darl.Lineage.Bot;
using DarlCommon;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Darl.Thinkbase
{
    public enum GraphProcess { seek, discover };

    public interface IGraphHandler
    {

        Task<KnowledgeState> Discover(string userId, string KnowledgeGraphName, string subjectId, List<string> lineages, StringBuilder log, FuzzyTime? currentTime);
        Task<(List<InteractTestResponse>, DarlVar?)> GraphPass(string userId, string graphName, string subjectId, string targetId, List<string> paths, string completionLineage, List<DarlVar> values, DarlVar? pending, GraphProcess graphProces);
        Task<List<InteractTestResponse>> InterpretText(string userId, string graphName, string subjectId, DarlVar conversationData);
        Task<KnowledgeState> Seek(KnowledgeState ks, string targetId, List<string> paths, string completionLineage);
    }
}