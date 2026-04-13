/// <summary>
/// </summary>

﻿using DarlCommon;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Darl.Lineage.Bot
{
    public interface ITrigger
    {
        Task TriggerEvent(List<DarlVar> values, RuleForm rf, string user, ServiceConnectivity? serv = null);
    }
}
