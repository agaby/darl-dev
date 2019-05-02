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
        private IConnectivity _connectivity;


        TelemetryClient telemetry = new TelemetryClient();

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="rep"></param>
        public FormApi(IMemoryCache cache, IConnectivity connectivity)
        {
            _cache = cache;
            _connectivity = connectivity;
        }

        /// <summary>
        /// Get the first or next set of questions.
        /// </summary>
        /// <param name="id">Either the Guid of a Rule set for the first fetch in a sequence or the ieToken returned in a previous set.</param>
        /// <param name="questCount">The question count. (optional)</param>
        /// <returns>
        /// The set of questions, or responses
        /// </returns>
        public async Task<QuestionSetProxy> Get(string id, int questCount = 1)
        {
            try
            {
                var language = string.Empty;
                QuestionCache qstate = null;
                RuleForm formResources = null;
                try
                {
                    if (!GetCaches(id, out qstate, out formResources))
                        return null;
                }
                catch
                {
                    return null;
                }
                //at this point the session data must be available
                qstate.languageSelection = language;
                qstate.requestedQuestions = questCount;
                return await form.Start(formResources, qstate);
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

            //QuestionSetProxy questionsetproxy = new QuestionSetProxy();
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
                QuestionCache qstate = null;
                RuleForm formResources = null;
                if (!GetCaches(id, out qstate, out formResources))
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


        private async Task<RuleForm> GetFormCache(string redirectAddress, QuestionCache qstate)
        {
            RuleForm newForm = null;
            if (!_cache.TryGetValue(redirectAddress, out newForm))
            {//not in cache
                var rs = await _connectivity.GetRuleSet(redirectAddress);
                if (rs != null)
                {

                    return rs.Contents;
                }
            }
            return newForm;
        }


        /// <summary>
        /// Gets the caches.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="qstate">The session's data</param>
        /// <param name="formResources">The form resources, ruleset, texts and formatting.</param>
        /// <returns><c>true</c> if the form exists, <c>false</c> otherwise.</returns>
        /// <remarks>When starting a form the projectId is passed. On subsequent updates a session token is passed. 
        /// This fact is used cache the form for re-use across multiple sessions. Note that a re-used form may need to be cleared out.</remarks>
        private bool GetCaches(string id, out QuestionCache qstate, out RuleForm formResources)
        {
            qstate = null;
            formResources = null;
            if (string.IsNullOrEmpty(id))
            {
                return false;
            }
            dynamic qc;
            if (!this._cache.TryGetValue(id, out qc)) //formapi is only used for form testing, so ruleset must be cached.
            {
                return false;
            }
            else
            {
                if (qc is QuestionCache)//subsequent call to same form, same session
                {
                    qstate = qc as QuestionCache;
                    formResources = _cache.Get(qstate.projectId.ToString()) as RuleForm;
                }
                else if (qc is RuleForm)// call to existing form, new session.
                {
                    formResources = qc as RuleForm;
                    qstate = new QuestionCache { currentIteration = 0, SessionKey = Guid.NewGuid(), projectId = id, currentData = DarlVarExtensions.Convert(formResources.preload) };
                    _cache.Set(qstate.SessionKey.ToString(), qstate, new MemoryCacheEntryOptions { SlidingExpiration = new TimeSpan(1,0,0) });
                }
            }
            return true;
        }





        #endregion
    }
}
