using Darl.GraphQL.Models.Models;
using Darl.Lineage;
using Darl.Thinkbase;
using Darl.Thinkbase.Meta;
using DarlCommon;
using DarlCompiler;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Connectivity
{
    public class KGContainer : IKGTranslation
    {

        private IConfiguration _config;
        private ILicensing _licensing;
        private ILogger _logger;
        private string userId;
        private IConnectivity _conn;
        private DarlMetaRunTime metaRuntime = new DarlMetaRunTime(new MetaStructureHandler());
        private IGraphProcessing _graph;


        public KGContainer(IConfiguration config, ILogger<KGContainer> logger, ILicensing licensing, IConnectivity conn, IGraphProcessing graph)
        {
            _logger = logger;
            _licensing = licensing;
            _config = config;
            userId = _config["SINGLEUSERID"];
            _conn = conn;
            _graph = graph;
            DarlMetaRunTime.SetLicense(_config["licensing:darlMetaLicense"]);
            Init().Wait();
        }

        private async Task Init()
        {
            var kgraphs = await _conn.GetKGraphsAsync(userId);

            foreach (var name in Assembly.GetExecutingAssembly().GetManifestResourceNames())
            {
                if (name.EndsWith(".graph"))
                {
                    var filename = name.Substring(name.Remove(name.Length - 6).LastIndexOf('.') + 1);
                    if(!kgraphs.Any( a => a.Name == filename))
                        await _conn.CreateKGraph(userId, filename);
                }
            }

        }

        public Task<bool> CheckEmail(string email, string ipaddress = "")
        {
            throw new NotImplementedException();
        }

        public Task<bool> CheckKey(string userId, string key)
        {
            return Task.FromResult<bool>(_licensing.CheckKey(key));
        }

        public Task<DarlUser> CreateAndRegisterNewUser(DarlUserInput user)
        {
            throw new NotImplementedException();
        }

        public Task<Contact> CreateContactAsync(Contact contact)
        {
            throw new NotImplementedException();
        }

        public Task<Default> CreateDefault(string name, string value)
        {
            throw new NotImplementedException();
        }

        public Task<string> CreateKey(string userId, string company, string email, DateTime endDate)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> CreateNewGraph(string userId, string modelName)
        {
            return await _graph.CreateNewGraph(userId, modelName);
        }

        public Task<DarlUser> CreateUserAsync(DarlUser user)
        {
            throw new NotImplementedException();
        }

        public Task<Collateral?> DeleteCollateral(string name)
        {
            throw new NotImplementedException();
        }

        public Task<Contact> DeleteContactAsync(string emailname)
        {
            throw new NotImplementedException();
        }

        public Task<Default?> DeleteDefault(string name)
        {
            throw new NotImplementedException();
        }

        public Task<string> ExportNoda(string userId, string graphName)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetCollateral(string name)
        {
            throw new NotImplementedException();
        }

        public Task<List<Collateral>> GetCollaterals()
        {
            throw new NotImplementedException();
        }

        public Task<Contact> GetContactByEmail(string email)
        {
            throw new NotImplementedException();
        }

        public Task<Contact> GetContactById(string Id)
        {
            throw new NotImplementedException();
        }

        public Task<List<Contact>> GetContacts()
        {
            throw new NotImplementedException();
        }

        public Task<List<Contact>> GetContactsByLastName(string lastName)
        {
            throw new NotImplementedException();
        }

        public Task<long> GetContactsCount(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<long> GetContactsDayCount(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<long> GetContactsMonthCount(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<IQueryable<Contact>> GetContactsQueryable()
        {
            throw new NotImplementedException();
        }

        public string GetCurrentUserId(object userContext)
        {
            return userId;
        }

        public Task<List<Default>> GetDefaults()
        {
            throw new NotImplementedException();
        }

        public Task<string> GetDefaultValue(string name)
        {
            throw new NotImplementedException();
        }

        public Task<DateTime> GetLastUpdate(string from, string to)
        {
            throw new NotImplementedException();
        }

        public Task<List<Contact>> GetRecentContacts()
        {
            throw new NotImplementedException();
        }

        public Task<List<DarlUser>> GetRecentUsers()
        {
            throw new NotImplementedException();
        }

        public Task<string> GetSuggestedRuleSet(string userId, string modelName, string objectId, string lineage)
        {
            throw new NotImplementedException();
        }

        public Task<DarlUser.AccountState?> GetUserAccountState(string customerId)
        {
            throw new NotImplementedException();
        }

        public Task<DarlUser> GetUserByApiKey(string apiKey)
        {
            throw new NotImplementedException();
        }

        public Task<DarlUser> GetUserById(string id)
        {
            throw new NotImplementedException();
        }

        public Task<DarlUser> GetUserByStripeId(string stripeId)
        {
            throw new NotImplementedException();
        }

        public Task<long> GetUserCount(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetUserIdFromAppId(string appId)
        {
            throw new NotImplementedException();
        }

        public Task<List<DarlUser>> GetUsers()
        {
            throw new NotImplementedException();
        }

        public Task<List<DarlUser>> GetUsersByEmail(string email)
        {
            throw new NotImplementedException();
        }

        public Task<string> RegisterForMarketing(string name, string email)
        {
            return Task.FromResult("Not available in container version.");
        }

        public Task<DateTime> SetLastUpdate(string from, string to)
        {
            throw new NotImplementedException();
        }

        public Task StoreSystemKG()
        {
            throw new NotImplementedException();
        }

        public Task<Collateral> UpdateCollateral(string name, string value)
        {
            throw new NotImplementedException();
        }

        public Task<Contact> UpdateContactAsync(Contact contact)
        {
            throw new NotImplementedException();
        }

        public Task<Default> UpdateDefault(string name, string value)
        {
            throw new NotImplementedException();
        }

        public Task<List<Update>> Updates()
        {
            return Task.FromResult(new List<Update>());
        }

        public Task UpdateUserAccountState(string id, DarlUser.AccountState paying)
        {
            throw new NotImplementedException();
        }

        public Task<string> UpdateUserAPIKey(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<DarlUser> UpdateUserAsync(string userId, DarlUserUpdate darlUserUpdate)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetTypeWordForLineage(string lineage, string isoLanguage = "en")
        {
            try
            {
                if (LineageLibrary.lineages.ContainsKey(lineage))
                    return Task.FromResult(LineageLibrary.lineages[lineage].typeWord);
                return Task.FromResult(string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Bad lineage lookup for lineage {lineage} message: {ex.Message}");
                return Task.FromResult(string.Empty);
            }
        }

        public async Task<List<LineageRecord>> GetLineagesForWord(string word, string isoLanguage = "en")
        {
            try
            {
                var offset = 0;
                return LineageLibrary.WordRecognizer(new List<string> { word }, ref offset, true);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Bad lineage lookup for word {word} message: {ex.Message}");
                return new List<LineageRecord>();
            }
        }

        public Task<List<DarlLintView>> LintDarlMeta(string darl)
        {
            var errorList = new List<DarlLintView>();
            int rowoffset = 0;
            int coloffset = 0;
            if (!string.IsNullOrEmpty(darl))
            {
                try
                {
                    var tree = metaRuntime.CreateTreeEdit(darl);
                    if (tree.HasErrors())
                    {
                        foreach (var pm in tree.ParserMessages)
                        {
                            errorList.Add(new DarlLintView { line_no = pm.Location.Line + 1 - rowoffset, column_no_start = pm.Location.Column + 1 - coloffset, column_no_stop = pm.Location.Column + 2 - coloffset, message = pm.Message, severity = pm.Level == ErrorLevel.Error ? "error" : "warning" });
                        }
                    }
                }
                catch (Exception ex)
                {

                }

            }
            return Task.FromResult(errorList);
        }
    }
}
