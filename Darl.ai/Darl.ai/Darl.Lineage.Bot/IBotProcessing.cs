using Darl.Thinkbase;
using DarlCommon;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Darl.Lineage.Bot
{
    public interface IBotProcessing
    {
        Task<List<InteractTestResponse>> InteractKGAsync(string userId, string KnowledgeGraphName, string conversationId, DarlVar conversationData);
        Task<KnowledgeState> Discover(string userId, string KnowledgeGraphName, string subjectId);
        Task<KnowledgeState> Seek(KnowledgeState ks, string? targetId,  List<string> paths, string completionLineage);
    }
}
