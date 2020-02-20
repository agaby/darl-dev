using DarlCommon;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Connectivity
{

    public enum SampleType {events, sampled }
    public interface ISimProcessing
    {
        Task<DaslSet> Simulate(string userId, string ruleset, DaslSet daslSet, SampleType sampleType);
    }
}
