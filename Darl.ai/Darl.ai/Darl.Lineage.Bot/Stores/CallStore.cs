/// </summary>

﻿using DarlCommon;
using DarlLanguage.Processing;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Darl.Lineage.Bot.Stores
{
    /// This class implements calls to other rule sets from within a Darl Rule set using a store.
    /// </summary>
    /// <remarks>Think about a fail-safe that empties the stack on a time out, failure, etc.</remarks>
    [Serializable]
    public class CallStore : ILocalStore
    {
        public string user { get; set; }

        public RuleForm currentRF { get; set; }

        public CallStore(IRuleFormInterface callInterface, string user)
        {
            this.ruleFormSource = callInterface;
            this.user = user;
        }

        [NonSerialized]
        public IRuleFormInterface ruleFormSource;


        /// returns a description of the addressed rule set.
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public async Task<DarlResult> ReadAsync(List<string> address)
        {
            string comp = string.IsNullOrEmpty(user) ? address[0] : $"{user}/{address[0]}";
            return new DarlResult("", await ruleFormSource.GetDetails(comp), DarlResult.DataType.textual);
        }

        /// Loads a new rule set by name and pushes on to stack.
        /// </summary>
        /// <param name="address">optional ruleset category</param>
        /// <param name="value">ruleset name</param>
        /// <returns>Nothing</returns>
        public async Task WriteAsync(List<string> address, DarlResult value)
        {
            string comp = string.IsNullOrEmpty(user) ? address[0] : user;//21/09/2017 fixes double /
            var rf = await ruleFormSource.Get(!string.IsNullOrEmpty(comp) ? $"{comp}/{value.stringConstant}" : value.stringConstant);
            if (rf != null)
            {
                currentRF = rf;
            }
        }
    }
}
