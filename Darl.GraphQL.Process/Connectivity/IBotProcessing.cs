using Darl.GraphQL.Models.Models;
using Darl.Lineage.Bot;
using DarlCommon;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Connectivity
{
    public interface IBotProcessing
    {
        Task<List<InteractTestResponse>> InteractAsync(string userId, string botModelName, string conversationId, DarlVar conversationData);
        Task<BotTestView> InteractTestAsync(string userId, string botModelName, string conversationId, string text, bool reset);
    }
}
