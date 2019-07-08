using Darl.GraphQL.Models.Models;
using Darl.Lineage;
using Darl.Lineage.Bot;
using Darl_standard.Darl.Forms;
using DarlCommon;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
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


        TelemetryClient telemetry = new TelemetryClient();

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="rep"></param>
        public FormApi(IDistributedCache cache)
        {
            _cache = cache;
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
                await SetCache(new CombinedCache {  questionCache = qstate, ruleForm = ruleSet.Contents});
                return await form.Start(ruleSet.Contents, qstate);
            }
            catch (Exception ex)
            {
                telemetry.TrackException(ex);
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
                await SetCache(co);
                return qsp;
            }
            catch (Exception ex)
            {
                telemetry.TrackException(ex);
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
                telemetry.TrackException(ex);
                throw ex;
            }
        }

        /// <summary>
        /// Performs any defined trigger actions if the form is complete
        /// </summary>
        /// <param name="id">the ieToken of a current completed session.</param>
        /// <returns></returns>
        public async Task<bool> Trigger(string id)
        {
            var co = await GetCaches(id);
            if (co == null)
                return false;
            if (co.ruleForm.trigger != null && Guid.TryParse(co.ruleForm.author, out Guid guid))
            {
                try
                {
                    //await _rep.HandleTrigger(fcache.trigger, qcache.currentData, fcache.author);
                }
                catch (Exception ex)
                {
                    telemetry.TrackException(ex);
                    return false;
                }
            }
            return true;
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
        static ITraceWriter traceWriter = new MemoryTraceWriter();

        static JsonSerializerSettings jss = new JsonSerializerSettings { TraceWriter = traceWriter, Converters = new List<JsonConverter>() { new StringEnumConverter { } } };

        public RuleForm ruleForm { get; set; }

        public QuestionCache questionCache { get; set;}

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
