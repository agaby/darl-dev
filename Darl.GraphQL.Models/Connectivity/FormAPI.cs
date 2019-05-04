using Darl.GraphQL.Models.Models;
using Darl.Lineage.Bot;
using Darl_standard.Darl.Forms;
using DarlCommon;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Connectivity
{
    /// <summary>
    /// performs form evaluation
    /// </summary>
    public class FormApi : IFormApi
    {
        Forms form = new Forms();

        private IMemoryCache _cache;


        TelemetryClient telemetry = new TelemetryClient();

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="rep"></param>
        public FormApi(IMemoryCache cache)
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
                _cache.Set(qstate.SessionKey.ToString(), (ruleSet.Contents, qstate), new MemoryCacheEntryOptions { SlidingExpiration = new TimeSpan(1, 0, 0) });
                //at this point the session data must be available
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
                QuestionCache qstate = null;
                RuleForm formResources = null;
                if (!GetCaches(questionsetproxy.ieToken, out qstate, out formResources))
                    return null;
                //at this point the session data must be available
                //TO DO, handle redirect
                var qsp = await form.Next(questionsetproxy, formResources, qstate);
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
                if (!GetCaches(id, out QuestionCache qstate, out RuleForm formResources))
                    return null;
                return await form.Back(formResources, qstate);
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

            QuestionCache qcache;
            if (!_cache.TryGetValue(id.ToString(), out qcache))
                return false;
            RuleForm fcache;
            if (!_cache.TryGetValue(qcache.projectId, out fcache))
                return false;
            if (fcache.trigger != null && Guid.TryParse(fcache.author, out Guid guid))
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
        private bool GetCaches(string id, out QuestionCache qstate, out RuleForm formResources)
        {
            qstate = null;
            formResources = null;
            if (string.IsNullOrEmpty(id))
            {
                return false;
            }
            if (!_cache.TryGetValue<(RuleForm, QuestionCache)>(id, out (RuleForm, QuestionCache) values))
                return false;
            formResources = values.Item1;
            qstate = values.Item2;
            return true;
        }





        #endregion
    }

}
