using Darl.GraphQL.Models.Models;
using Darl.Lineage.Bot;
using Darl_standard.Darl.Forms;
using DarlCommon;
using Datl.Language;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Connectivity
{
    /// <summary>
    /// Performs form evaluation 
    /// Arranged as a singleton. Each ruleset and questionnaire state is cached and retrieved.
    /// </summary>
    public class FormApi : IFormApi
    {
        Forms form = new Forms();

        private IDistributedCache _cache;
        private ITrigger _trigger;


        private ILogger _logger;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="rep"></param>
        public FormApi(IDistributedCache cache, ITrigger trigger, ILogger logger)
        {
            _cache = cache;
            _trigger = trigger;
            _logger = logger;
        }

        /// <summary>
        /// Get the first or next set of questions.
        /// </summary>
        /// <param name="id">Either the Guid of a Rule set for the first fetch in a sequence or the ieToken returned in a previous set.</param>
        /// <param name="questCount">The question count. (optional)</param>
        /// <returns>
        /// The set of questions, or responses
        /// </returns>
        public async Task<QuestionSetProxy> Get(RuleSet ruleSet, string language = "en", int questCount = 1)
        {
            try
            {
                var qstate = new QuestionCache { currentIteration = 0, SessionKey = Guid.NewGuid(), projectId = ruleSet.Name, currentData = DarlVarExtensions.Convert(ruleSet.Contents.preload) };
                await SetCache(new CombinedCache {  questionCache = qstate, ruleForm = ruleSet.Contents, userId = ruleSet.userId, serviceConnectivity = ruleSet.serviceConnectivity});
                return await form.Start(ruleSet.Contents, qstate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, nameof(Get));
                throw ex;
            }
        }

        /// <summary>
        /// Post the results to the last set of questions
        /// </summary>
        /// <param name="questionsetproxy">The questionsetproxy.</param>
        /// <returns>
        /// new set of questions or responses
        /// </returns>
        /// <remarks>
        /// No object is created in this call, this is a data exchange, so a 200 status is returned for correct operation.
        /// </remarks>
        public async Task<QuestionSetProxy> Post(QuestionSetProxy questionsetproxy)
        {

            try
            {
                var co = await GetCaches(questionsetproxy.ieToken);
                if (co == null)
                    return null; 
                //TO DO, handle redirect
                var qsp = await form.Next(questionsetproxy, co.ruleForm, co.questionCache);
                if(qsp.complete && !co.triggered)
                {
                    await Trigger(co);
                    co.triggered = true; //one shot.
                }
                await SetCache(co);
                return qsp;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, nameof(Post));
                throw ex;
            }

        }

        /// <summary>
        /// Deletes the last set of responses, acting as a "back" function.
        /// </summary>
        /// <param name="id">the ieToken of the sequence.</param>
        public async Task<QuestionSetProxy> Delete(string id)
        {
            try
            {
                var co = await GetCaches(id);
                if (co == null)
                    return null;
                var res =  await form.Back(co.ruleForm, co.questionCache);
                await SetCache(co);
                return res;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, nameof(Delete));
                throw ex;
            }
        }

        /// <summary>
        /// Performs any defined trigger actions if the form is complete
        /// </summary>
        /// <param name="id">the ieToken of a current completed session.</param>
        /// <returns></returns>
        private async Task<bool> Trigger(CombinedCache co)
        {
            if (co.ruleForm.trigger != null )
            {
                try
                {
                    await _trigger.TriggerEvent(DarlVarExtensions.Convert(co.questionCache.currentData), co.ruleForm, co.userId, co.serviceConnectivity );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, nameof(Trigger));
                    return false;
                }
            }
            return true;
        }

        public async Task<QuestionSetProxy> CreateDynamicRuleSetEditor(RuleSet ruleset, RuleSet template)
        {
            //insert key elements from the ruleset into the template
            var inserts = new Dictionary<string, string>();
            inserts.Add("inputs",string.Join(',', ruleset.Contents.format.InputFormatList.Select(a => a.Name)));
            inserts.Add("outputs",string.Join(',', ruleset.Contents.format.OutputFormatList.Select(a => a.Name)));
            inserts.Add("texts","\"" + string.Join("\",\"", ruleset.Contents.language.LanguageList.Select(a => a.Name)) + "\"");
            var sb = new StringBuilder();
            bool numericsExist = false;
            if(ruleset.Contents.format.InputFormatList.Where(a => a.InType == InputFormat.InputType.numeric).Any())
            {
                sb.Append("if inputs is ");
                sb.Append(string.Join(" or inputs is ", ruleset.Contents.format.InputFormatList.Where(a => a.InType == InputFormat.InputType.numeric).Select(a => a.Name)));
                sb.Append(" then numericIO will be true; \n");
                numericsExist = true;
            }
            if (ruleset.Contents.format.OutputFormatList.Where(a => a.OutputType == OutputFormat.OutType.numeric).Any())
            {
                sb.Append("if outputs is ");
                sb.Append(string.Join(" or outputs is ", ruleset.Contents.format.OutputFormatList.Where(a => a.OutputType == OutputFormat.OutType.numeric).Select(a => a.Name)));
                sb.Append(" then numericIO will be true; \n");
                numericsExist = true;
            }
            if(numericsExist)
            {
                sb.Append("otherwise ");
            }
            sb.Append("if anything then numericIO will be false;\n");

            bool textualsExist = false;
            if(ruleset.Contents.format.InputFormatList.Where(a => a.InType == InputFormat.InputType.textual).Any())
            {
                sb.Append("if inputs is ");
                sb.Append(string.Join(" or inputs is ", ruleset.Contents.format.InputFormatList.Where(a => a.InType == InputFormat.InputType.textual).Select(a => a.Name)));
                sb.Append(" then textualIO will be true; \n");
                textualsExist = true;
            }
            if (ruleset.Contents.format.OutputFormatList.Where(a => a.OutputType == OutputFormat.OutType.textual).Any())
            {
                sb.Append("if outputs is ");
                sb.Append(string.Join(" or outputs is ", ruleset.Contents.format.OutputFormatList.Where(a => a.OutputType == OutputFormat.OutType.textual).Select(a => a.Name)));
                sb.Append(" then textualIO will be true; \n");
                textualsExist = true;
            }
            if (textualsExist)
            {
                sb.Append("otherwise ");
            }
            sb.Append("if anything then textualIO will be false;\n");
            inserts.Add("rules", sb.ToString());

            //insert text into template 
            var tp = new TextProcess();
            template.Contents.darl = tp.Parse(template.Contents.darl, inserts);

            await template.Contents.UpdateFromCode();


            return await Get(template);
                
        }


        #region support methods


        /// <summary>
        /// Gets the caches.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="qstate">The session's data</param>
        /// <param name="formResources">The form resources, ruleset, texts and formatting.</param>
        /// <returns><c>true</c> if the form exists, <c>false</c> otherwise.</returns>
        /// <remarks>When starting a form the projectId is passed. On subsequent updates a session token is passed. 
        /// The ruleform is also cached, but used solely for this questionnaire.</remarks>
        private async Task<CombinedCache> GetCaches(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }
            var c = await _cache.GetStringAsync(id);
            if(!string.IsNullOrEmpty(c))
            {
                return CombinedCache.Factory(c);
            }
            return null;
        }

        private async Task SetCache(CombinedCache co)
        {
            //conversations held for one hour.
            var coz = co.ToString();
            await _cache.SetStringAsync(co.questionCache.SessionKey.ToString(), coz, new DistributedCacheEntryOptions { SlidingExpiration = new TimeSpan(1,0,0) });
        }

 
        #endregion
    }

    public class CombinedCache
    {
        static ITraceWriter traceWriter = new MemoryTraceWriter(); //new DiagnosticsTraceWriter(); //

        static JsonSerializerSettings jss = new JsonSerializerSettings { TraceWriter = traceWriter, Converters = new List<JsonConverter>() { new StringEnumConverter { } }, ReferenceLoopHandling = ReferenceLoopHandling.Ignore };

        public RuleForm ruleForm { get; set; }

        public QuestionCache questionCache { get; set;}

        public string userId { get; set; }

        public ServiceConnectivity serviceConnectivity { get; set; }

        public bool triggered { get; set; } = false;


        public override string ToString()
        {
             return JsonConvert.SerializeObject(this, jss);               
        }

        public static CombinedCache Factory(string source)
        {
            return JsonConvert.DeserializeObject<CombinedCache>(source,jss);
        }
    }

}
