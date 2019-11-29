using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DarlCommon;
using Dasl.TemporalDb;
using DaslLanguage;
using GraphQL;
using Microsoft.ApplicationInsights;

namespace Darl.GraphQL.Models.Connectivity
{
    public class SimProcessing : ISimProcessing
    {
        DaslRunTime sruntime = new DaslRunTime();
        private TelemetryClient telemetryClient = new TelemetryClient();
        private IConnectivity _connectivity;

        public SimProcessing(IConnectivity connectivity)
        {
            _connectivity = connectivity;
        }

        public async Task<DaslSet> Simulate(string userId, string ruleset, DaslSet daslSet, SampleType sampleType)
        {
            var rs = await _connectivity.GetRuleSet(userId, ruleset);
            if(rs == null)
            {
                throw new ExecutionError($"ruleset {ruleset} not found for user {userId}");
            }
            var tree = sruntime.CreateTree(rs.Contents.darl);
            if (tree.HasErrors())
            {
                throw new ExecutionError($"ruleset {ruleset} has errors - use linter");
            }
            var el = new EventList();
            el.events = daslSet.events;
            el.sample = daslSet.sampleTime;
            var sampled = sampleType == SampleType.events ? el.GetEventData() : el.SampleData();
            var res = await sruntime.Simulate(sampled, sampled.Count, tree);
            telemetryClient.TrackEvent($"Simulate", new Dictionary<string, string> { { nameof(userId), userId }, { nameof(ruleset), ruleset }, {"usage", res.Count.ToString() } });
            return new DaslSet { events = el.ConvertToEvents(res), description = daslSet.description, sampleTime = daslSet.sampleTime };
        }
    }
}
