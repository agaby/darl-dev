using DarlCommon;
using DarlLanguage.Processing;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Darl.Lineage.Bot
{
    public interface IRuleSetHandler
    {
        ITrigger Trigger { get; set; }

        void Back(List<DarlVar> values);
        bool CanGoBack();
        Task<List<InteractTestResponse>> RuleSetPass(List<DarlVar> values, Dictionary<string, ILocalStore> stores, ServiceConnectivity? service = null);
    }
}
