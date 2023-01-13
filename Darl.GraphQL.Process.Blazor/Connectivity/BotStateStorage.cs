using Darl.Lineage.Bot;
using Microsoft.Extensions.Caching.Distributed;
using ProtoBuf;

namespace Darl.GraphQL.Process.Blazor.Connectivity
{
    public class BotStateStorage : IBotStateStorage
    {

        readonly IDistributedCache _cache;

        public BotStateStorage(IDistributedCache cache)
        {
            _cache = cache;
        }

        public Task ClearBotStates(string userId, string graphName)
        {
            throw new NotImplementedException();
        }

        public async Task<BotState?> GetBotState(string userId, string conversationId)
        {
            var blob = await _cache.GetAsync(conversationId);
            if (blob != null)
            {
                using (var ms = new MemoryStream(blob))
                {
                    ms.Position = 0;
                    return Serializer.Deserialize<BotState>(ms);
                }
            }
            return null;
        }

        public async Task SetBotState(string userId, string conversationId, BotState state)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                Serializer.Serialize<BotState>(ms, state);
                ms.Position = 0;
                await _cache.SetAsync(conversationId, ms.ToArray(), new DistributedCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(30)));
            }
        }
    }
}
