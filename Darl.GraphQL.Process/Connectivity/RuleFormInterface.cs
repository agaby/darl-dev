using Darl.Lineage.Bot.Stores;
using DarlCommon;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Connectivity
{
    public class RuleFormInterface : IRuleFormInterface
    {
        IConnectivity _connectivity;
        public RuleFormInterface(IConnectivity connectivity)
        {
            _connectivity = connectivity;
        }
        public async Task<RuleForm> Get(string address)
        {
            var names = SplitAddress(address);
            var rf = await _connectivity.GetRuleSet(names.Item1, names.Item2);
            return rf.Contents;
        }

        public async Task<string> GetCollateral(string user, string v)
        {
            return await _connectivity.GetCollateral(user, v);
        }

        public async Task<string> GetDetails(string address)
        {
            var rf = await Get(address);
            return $"Ruleset: {rf.name}, Description: {rf.description}";

        }

        //not used
        public async Task<List<string>> GetListings()
        {
            throw new NotImplementedException();
        }

        private (string, string) SplitAddress(string address)
        {
            var loc = address.IndexOf('/');
            if (loc > 0)
            {
                var user = address.Substring(0, loc);
                var name = address.Substring(loc + 1);
                return (user, name);
            }
            return ("", address);
        }
    }
}
