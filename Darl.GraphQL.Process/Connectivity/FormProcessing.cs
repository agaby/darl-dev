using Darl.GraphQL.Models.Models;
using Darl.GraphQL.Models.Schemata;
using Darl.GraphQL.Process.Models.Alexa;
using DarlCommon;
using GraphQL;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Connectivity
{
    public class FormProcessing : IFormProcessing
    {
        private IConnectivity _connectivity;
        private IFormApi _formApi;
        private IConfiguration _config;
        private ILogger _logger;


        public FormProcessing(IConnectivity connectivity, IFormApi formApi, IConfiguration config, ILogger<FormProcessing> logger)
        {
            _connectivity = connectivity;
            _formApi = formApi;
            _config = config;
            _logger = logger;
        }

        public async Task<QuestionSetProxy> BacktrackQuestionnaire(string ieToken)
        {
            return await _formApi.Delete(ieToken);
        }

        public async Task<QuestionSetProxy> BeginDynamicQuestionnaire(string userId, string selector, DQType dqType)
        {
            switch (dqType)
            {
                case DQType.rule_edit:
                    {
                        var rs = await _connectivity.GetRuleSet(userId, selector);
                        var tp = await _connectivity.GetRuleSet(_config["boaiuserid"], "ruleseteditor.rule");
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
                _logger.LogWarning($"{nameof(BeginQuestionnaire)}: {userId}, {ruleSetName}, {qsp.ieToken}");
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
            _logger.LogWarning($"{nameof(ContinueQuestionnaire)}: {r.ieToken}");
            return r;
        }

        /// <summary>
        /// Get a Json string to use in setting up intents and slots in an Alexa Skill
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="name"></param>
        /// <param name="invocationName"></param>
        /// <returns></returns>
        public async Task<InteractionModel> GetAlexaInteractionModel(string userId, string name, string invocationName)
        {
            var rs = await _connectivity.GetRuleSet(userId, name);
            if(rs == null)
            {
                throw new ExecutionError($"ruleset {name} does not exist.");
            }
            if(string.IsNullOrEmpty(invocationName))
            {
                throw new ExecutionError($"Invocation name should not be empty.");
            }

            var im = new InteractionModel();
            im.languageModel.invocationName = invocationName;
            var categoriesVIntents = new Dictionary<string, string>();
            //iterate over inputs
            foreach (var input in rs.Contents.format.InputFormatList)
            {
                var intent = new Intent();
                switch(input.InType)
                {
                    case InputFormat.InputType.numeric:
                        {
                            if(input.ShowSets)
                            {
                                foreach (var c in input.Categories)
                                {
                                    DetectYesNoNodes(c, im.languageModel.intents);
                                    var catName = $"{input.Name}.{c}";
                                    var displayedTextNode = rs.Contents.language.LanguageList.Where(a => a.Name == catName).FirstOrDefault();
                                    if (displayedTextNode != null)
                                    {
                                        DetectYesNoNodes(displayedTextNode.Name, im.languageModel.intents);
                                    }
                                }
                            }
                            else 
                            { 
                                intent.name = $"{input.Name}Intent";
                                intent.slots.Add(new Slot { type = "AMAZON.NUMBER", name = input.Name });
                                intent.samples.Add($"{{{input.Name}}}");
                                im.languageModel.intents.Add(intent);
                            }
                        }
                        break;
                    case InputFormat.InputType.temporal:
                        {
                            intent.name = $"{input.Name}Intent";
                            intent.slots.Add(new Slot { type = "AMAZON.DATE", name = input.Name });
                            intent.samples.Add($"{{{input.Name}}}");
                            im.languageModel.intents.Add(intent);
                        }
                        break;
                    case InputFormat.InputType.categorical:
                        {

                            //for the time being just collect yes/no intents
                            foreach (var c in input.Categories)
                            {
                                DetectYesNoNodes(c, im.languageModel.intents);
                                var catName = $"{input.Name}.{c}";
                                var displayedTextNode = rs.Contents.language.LanguageList.Where(a => a.Name == catName).FirstOrDefault();
                                if(displayedTextNode != null)
                                {
                                    DetectYesNoNodes(displayedTextNode.Name, im.languageModel.intents);
                                }
                            }
                        }
                        break;
                    case InputFormat.InputType.textual:
                        {
                            //textual inputs are almost always just informative - i.e. not dialog - stopping

                        }
                        break;
                }

            }
            //add built in intents
            im.languageModel.intents.Add(new Intent { name = "AMAZON.FallbackIntent" });
            im.languageModel.intents.Add(new Intent { name = "AMAZON.NavigateHomeIntent" });
            im.languageModel.intents.Add(new Intent { name = "AMAZON.CancelIntent" });
            im.languageModel.intents.Add(new Intent { name = "AMAZON.HelpIntent" });
            im.languageModel.intents.Add(new Intent { name = "AMAZON.StopIntent" });

            //Add custom intents

            return im;
        }

        private bool DetectYesNoNodes(string c, List<Intent> intents)
        {
            //many categorical inputs will just be yes/no choices. 
            if (c.Equals("true", StringComparison.InvariantCultureIgnoreCase) ||
                c.Equals("yes", StringComparison.InvariantCultureIgnoreCase))
            {
                if (!intents.Any(a => a.name == "AMAZON.YesIntent"))
                {
                    intents.Add(new Intent { name = "AMAZON.YesIntent" });
                }
                return true;
            }
            if (c.Equals("false", StringComparison.InvariantCultureIgnoreCase) ||
                c.Equals("no", StringComparison.InvariantCultureIgnoreCase))
            {
                if (!intents.Any(a => a.name == "AMAZON.NoIntent"))
                {
                    intents.Add(new Intent { name = "AMAZON.NoIntent" });
                }
                return true;
            }
            return false;
        }

    }
}
