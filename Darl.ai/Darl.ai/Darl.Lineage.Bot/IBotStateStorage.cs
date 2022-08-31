using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Darl.Lineage.Bot
{
    public interface IBotStateStorage
    {
        Task<BotState?> GetBotState(string conversationId);

        Task SetBotState(string conversationId, BotState state);
    }
}
