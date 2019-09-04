using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Darl.GraphQL.Models.Models;
using Darl.GraphQL.Models.Schemata;
using DarlCommon;
using GraphQL;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Options;

namespace Darl.GraphQL.Models.Connectivity
{
    public class FormProcessing : IFormProcessing
    {
        private IConnectivity _connectivity;
        private IFormApi _formApi;
        private IOptions<AppSettings> _opt;
        private TelemetryClient telemetryClient = new TelemetryClient();


        public FormProcessing(IConnectivity connectivity, IFormApi formApi, IOptions<AppSettings> optionsAccessor)
        {
            _connectivity = connectivity;
            _formApi = formApi;
            _opt = optionsAccessor;
        }

        public async Task<QuestionSetProxy> BacktrackQuestionnaire(string ieToken)
        {
            return await _formApi.Delete(ieToken);
        }

        public async Task<object> BeginDynamicQuestionnaire(string userId, string selector, DQType dqType)
        {
            switch (dqType)
            {
                case DQType.rule_edit:
                    {
                        var rs = await _connectivity.GetRuleSet(userId, selector);
                        var tp = await _connectivity.GetRuleSet(_opt.Value.boaiuserid, "ruleseteditor.rule");
                        if (rs != null)
                            return await _formApi.CreateDynamicRuleSetEditor(rs, tp);
                        return null;
                    }
            }
            return null;
        }
    

        public async Task<QuestionSetProxy> BeginQuestionnaire(string userId, string ruleSetName, string language = "en", int questCount = 1)
        {
            var rs = await _connectivity.GetRuleSet(userId, ruleSetName);
            if (rs != null)
            {
                var qsp = await _formApi.Get(rs, language, questCount);
                telemetryClient.TrackEvent($"Questionnaire interaction started. userId = {userId}, Ruleset name = {ruleSetName}, id = {qsp.ieToken}");
                return qsp;
            }
            return null;
        }

        public async Task<QuestionSetProxy> ContinueQuestionnaire(QuestionSetInput responses)
        {
            QuestionSetProxy r = null;
            try
            {
                var resp = new QuestionSetProxy { ieToken = responses.ieToken, questions = new List<QuestionProxy>() };
                foreach (var i in responses.questions)
                {
                    resp.questions.Add(new QuestionProxy { dResponse = i.dResponse, reference = i.reference, sResponse = i.sResponse, qtype = (int)i.qType });
                }
                r = await _formApi.Post(resp);
            }
            catch (Exception ex)
            {
                throw new ExecutionError($"Internal error", ex);
            }
            if (r == null)
            {
                throw new ExecutionError($"ieToken {responses.ieToken} not found. Questionnaire timed out?");
            }
            telemetryClient.TrackEvent($"Questionnaire interaction continued.  id = {r.ieToken}");
            return r;
        }
    }
}
