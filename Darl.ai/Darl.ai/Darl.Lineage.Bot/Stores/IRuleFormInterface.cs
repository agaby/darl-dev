/// <summary>
/// IRuleFormInterface.cs - Core module for the Darl.dev project.
/// </summary>

﻿using DarlCommon;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Darl.Lineage.Bot.Stores
{
    public interface IRuleFormInterface
    {
        /// <summary>
        /// Get a RuleForm object by name
        /// </summary>
        /// <param name="address"></param>
        /// <returns>the ruleForm object, null if not found</returns>
        Task<RuleForm> Get(string address);

        Task<List<string>> GetListings();

        Task<string> GetDetails(string address);

        Task<string> GetCollateral(string user, string v);
    }
}