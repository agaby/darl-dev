using System.Collections.Generic;
using System.Threading.Tasks;
using DarlLanguage.Processing;

namespace Darl.Lineage.Bot.Stores
{
    internal class CollateralStore : ILocalStore
    {

        IRuleFormInterface coll;
        readonly string _user;

        public CollateralStore(IRuleFormInterface collInterface, string user)
        {
            coll = collInterface;
            _user = user;
        }

        public async Task<DarlResult> ReadAsync(List<string> address)
        {
            var s = await coll.GetCollateral(_user, address[0]);
            if (s == null)
                return new DarlResult("", "Can't find that collateral; check the name.", DarlResult.DataType.textual);
            return new DarlResult("", s, DarlResult.DataType.textual);
        }

        public Task WriteAsync(List<string> address, DarlResult value)
        {
            return Task.CompletedTask;
        }
    }
}