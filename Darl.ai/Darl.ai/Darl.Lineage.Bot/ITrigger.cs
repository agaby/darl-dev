using DarlCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Darl.Lineage.Bot
{
    public interface ITrigger
    {
        Task TriggerEvent(List<DarlVar> values, RuleForm rf, string user, ServiceConnectivity? serv = null);
    }
}
