using Darl.Common;
using Darl.GraphQL.Models.Middleware;
using Darl.GraphQL.Models.Models;
using Darl.Lineage;
using Darl.Thinkbase;
using GraphQL;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Queue;

namespace Darl.GraphQL.Models.Connectivity
{
    /// <summary>
    /// Transforms between GraphQL processes and KGs
    /// </summary>
    public class KGTranslation : IKGTranslation
    {
        private IConfiguration _config;
        private ILogger<KGTranslation> _logger;
        private IGraphProcessing _graph;
        private IMetaStructureHandler _meta;
        private IProducts _prods;
        private ICheckEmail _checkEmail;

        //fill in with single call to model at startup
        private string personObjectId { get; set; } = string.Empty;
        private string defaultObjectId { get; set; } = string.Empty;
        private string collateralObjectId { get; set; } = string.Empty;
        private string updateObjectId { get; set; } = string.Empty;


        public static string backofficeKGComp = String.Empty;
        private static string backofficeKG = String.Empty;
        private static string sourceLineage = "noun:01,4,04,02,21,16";
        private static string destinationLineage = "noun:01,0,0,15,15,3";
        private static string processLineage = "noun:00,4";
        private static string defaultLineage = "noun:01,0,0,15,07,02,06,05";//constant
        private static string valueLineage = "noun:01,4,04,02,07,01";//text
        private static string collateralLineage = "noun:00,1,00,3,10,09,07";//document
        private static string firstNameLineage = "noun:01,3,14,01,06,13";//first name
        private static string lastNameLineage = "noun:01,3,14,01,06,11";//surname
        private static string emailLineage = "noun:01,0,2,00,38,00,06,1";//email
        private static string phoneLineage = "noun:01,4,07,01";//phone
        private static string occupationLineage = "noun:01,0,2,00,23,19";//occupation
        private static string noteLineage = "noun:01,4,05,21,28,1";//note
        private static string companyLineage = "noun:01,2,07,10";//organization
        private static string countryLineage = "noun:01,2,06,35";//nation
        private static string sectorLineage = "noun:01,0,0,15,07,02,04,1,02,1";//sector
        private static string keyLineage = "noun:01,4,09,01,7,3,0";//key
        private static string subscriptionLineage = "noun:01,0,2,00,34,6,1,5,0";
        private static string idLineage = "noun:01,4,09,01,7,3,5";
        private static string stateLineage = "noun:01,1,00";
        private static string typeLineage = "noun:01,0,0,15,07,02,02,0,01";
        private static string existenceLineage = "noun:01,5,03,3,018";//life

        public KGTranslation(ILogger<KGTranslation> logger, IConfiguration config, IGraphProcessing graph, IMetaStructureHandler meta, IProducts prods, ICheckEmail checkEmail)
        {
            _config = config;
            _logger = logger;
            _graph = graph;
            _meta = meta;
            _prods = prods;
            _checkEmail = checkEmail;
            backofficeKG = _config["AppSettings:BackOfficeKG"];
            backofficeKGComp = _config["AppSettings:boaiuserid"] + '_' + backofficeKG;
            GetObjectIds().Wait();
        }

        private async Task GetObjectIds()
        {
            var model = await _graph.GetModel(_config["AppSettings:boaiuserid"], backofficeKG);
            foreach (var v in model.vertices)
            {

                if (v.Value.lineage == defaultLineage)
                {
                    defaultObjectId = v.Value.id;
                }
                else if (v.Value.lineage == collateralLineage)
                {
                    collateralObjectId = v.Value.id;
                }
                else if (v.Value.lineage == _meta.CommonLineages["person"])
                {
                    personObjectId = v.Value.id;
                }
                else if (v.Value.lineage == processLineage)
                {
                    updateObjectId = v.Value.id;
                }
            }
        }

        #region SystemUpdates

        public async Task<List<Update>> Updates()
        {
            var kslist = await _graph.GetKnowledgeStatesByType(_config["AppSettings:boaiuserid"], updateObjectId, backofficeKG);
            return kslist.Select(a => ConvertUpdate(a, updateObjectId)).ToList();
        }

        Update ConvertUpdate(KnowledgeState ks, string objectId)
        {
            var ex = GetExistence(ks, updateObjectId);
            if (ex == null)
                return new Update();
            var update = ex.Last();
            if (update == null)
                return new Update();
            return new Update
            {
                from = GetAttributeValue(ks, updateObjectId, sourceLineage),
                to = GetAttributeValue(ks, updateObjectId, destinationLineage),
                updated = update.dateTime
            };
        }

        public async Task<DateTime> GetLastUpdate(string from, string to)
        {
            var obj = await GetUpdate(from, to);
            if (obj != null)
            {
                var last = GetExistence(obj, updateObjectId).Last();
                return last != null ? last.dateTime : DateTime.MinValue;
            }
            return DateTime.MinValue;
        }

        public async Task<DateTime> SetLastUpdate(string from, string to)
        {
            var ks = await GetUpdate(from, to);
            if (ks != null)
            {
                UpdateExistence(ks, updateObjectId, new List<DarlTime?> { new DarlTime(DateTime.UtcNow) });
            }
            else
            {
                var goi = new KnowledgeStateInput
                {
                    subjectId = $"{from}/{to}",
                    knowledgeGraphName = backofficeKG,
                    data = new List<StringListGraphAttributeInputPair>
                    {
                        new StringListGraphAttributeInputPair{
                            name = updateObjectId,
                            value = new List<GraphAttributeInput> {
                                new GraphAttributeInput {
                                    name = "existence",
                                    lineage = existenceLineage,
                                    type = GraphAttribute.DataType.temporal,
                                    existence = new List<DarlTime?> { new DarlTime(DateTime.UtcNow) },
                                    confidence = 1.0
                                },
                                new GraphAttributeInput {
                                    name = "from",
                                    lineage = sourceLineage,
                                    type = GraphAttribute.DataType.textual,
                                    value = from,
                                    confidence = 1.0
                                },
                                new GraphAttributeInput {
                                    name = "to",
                                    lineage = destinationLineage,
                                    type = GraphAttribute.DataType.textual,
                                    value = from,
                                    confidence = 1.0
                                }
                            }
                        }
                    }
                };
                await _graph.CreateKnowledgeState(_config["AppSettings:boaiuserid"], goi);
            }
            return DateTime.UtcNow;
        }

        private async Task<KnowledgeState?> GetUpdate(string from, string to)
        {
            return await _graph.GetKnowledgeState(_config["AppSettings:boaiuserid"], $"{from}/{to}", backofficeKG);
        }

        #endregion

        #region Defaults
        public async Task<List<Default>> GetDefaults()
        {
            var kslist = await _graph.GetKnowledgeStatesByType(_config["AppSettings:boaiuserid"], defaultObjectId, backofficeKG);
            return kslist.Select(a => ConvertDefault(a, defaultObjectId)).ToList();
        }

        private Default ConvertDefault(KnowledgeState ks, string objectId)
        {
            return new Default
            {
                Name = ks.subjectId,
                Value = GetAttributeValue(ks, objectId, valueLineage)
            };
        }

        public async Task<string> GetDefaultValue(string name)
        {
            var ks = await _graph.GetKnowledgeState(_config["AppSettings:boaiuserid"], name, backofficeKG);
            if(ks != null)
                return GetAttributeValue(ks, defaultObjectId, valueLineage) ?? string.Empty;
            return string.Empty;
        }

        public async Task<Default> CreateDefault(string name, string value)
        {
            var goi = new KnowledgeStateInput
            {
                subjectId = name,
                knowledgeGraphName = backofficeKG,
                data =  new List<StringListGraphAttributeInputPair>
                {
                    new StringListGraphAttributeInputPair{
                        name =  defaultObjectId,  
                        value =   new List<GraphAttributeInput> {
                            new GraphAttributeInput {
                                name = "value",
                                lineage = valueLineage,
                                type = GraphAttribute.DataType.textual,
                                value = value,
                                confidence = 1.0
                            }
                        }
                    }
                }
            };
            await _graph.CreateKnowledgeState(_config["AppSettings:boaiuserid"], goi);
            return new Default { Name = name, Value = value };
        }

        public async Task<Default?> DeleteDefault(string name)
        {
            await _graph.DeleteKnowledgeState(_config["AppSettings:boaiuserid"], name, backofficeKG);
            return null;
        }

        public async Task<Default> UpdateDefault(string name, string value)
        {
            var ks = await _graph.GetKnowledgeState(_config["AppSettings:boaiuserid"], name, backofficeKG);
            if (ks != null)
            {
                UpdateAttribute(ks, defaultObjectId, valueLineage, "value", value);
                return new Default { Name = name, Value = value };
            }
            return await CreateDefault(name, value);
        }

        #endregion

        #region Collateral

        public async Task<string> GetCollateral(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                var ks = await _graph.GetKnowledgeState(_config["AppSettings:boaiuserid"], name, backofficeKG);
                if(ks != null)
                    return GetAttributeValue(ks, collateralObjectId, valueLineage) ?? string.Empty;
            }
            return string.Empty;
        }

        public async Task<Collateral> UpdateCollateral(string name, string value)
        {
            var userId = _config["AppSettings:boaiuserid"];
            var ks = await _graph.GetKnowledgeState(userId, name, backofficeKG);
            if (ks != null)
            {
                UpdateAttribute(ks, collateralObjectId, valueLineage, "value", value, true);
                await _graph.SaveKSChanges(userId, collateralObjectId, ks);
                return new Collateral { Name = name, Value = value };
            }
            return await CreateCollateral(name, value);
        }

        private async Task<Collateral> CreateCollateral(string name, string value)
        {
            var goi = new KnowledgeStateInput
            {
                subjectId = name,
                knowledgeGraphName = backofficeKG,
                data = new List<StringListGraphAttributeInputPair>
                {
                    new StringListGraphAttributeInputPair{
                        name = collateralObjectId,  
                        value = new List<GraphAttributeInput> {
                            new GraphAttributeInput {
                                name = "value",
                                lineage = valueLineage,
                                type = GraphAttribute.DataType.textual,
                                value = value,
                                confidence = 1.0
                            }
                        }
                    }
                }
            };
            await _graph.CreateKnowledgeState(_config["AppSettings:boaiuserid"], goi);
            return new Collateral { Name = name, Value = value };
        }

        public async Task<Collateral?> DeleteCollateral(string name)
        {
            await _graph.DeleteKnowledgeState(_config["AppSettings:boaiuserid"], name, backofficeKG);
            return null;
        }

        public async Task<List<Collateral>> GetCollaterals()
        {
            var kslist = await _graph.GetKnowledgeStatesByType(_config["AppSettings:boaiuserid"], collateralObjectId, backofficeKG);
            return kslist.Select(a => ConvertCollateral(a, collateralObjectId)).ToList();
        }

        #endregion

        #region Contacts

        public async Task<List<Contact>> GetRecentContacts()
        {
            DateTime cutOff = DateTime.UtcNow - new TimeSpan(7, 0, 0, 0);
            var kslist = await _graph.GetKnowledgeStatesByType(_config["AppSettings:boaiuserid"], personObjectId, backofficeKG);
            return kslist.Where(a => GetExistenceStart(a, personObjectId) > cutOff).Select(a => ConvertContact(a, personObjectId)).ToList();
        }

        public async Task<IQueryable<Contact>> GetContactsQueryable()
        {
            try
            {
                var kslist = await _graph.GetKnowledgeStatesByType(_config["AppSettings:boaiuserid"], personObjectId, backofficeKG);
                return kslist.Select(a => ConvertContact(a, personObjectId)).AsQueryable();
            }
            catch (Exception ex)
            {

            }
            return new List<Contact>().AsQueryable();
        }

        public async Task<List<Contact>> GetContacts()
        {
            try
            {
                var kslist = await _graph.GetKnowledgeStatesByType(_config["AppSettings:boaiuserid"], personObjectId, backofficeKG);
                return kslist.Select(a => ConvertContact(a, personObjectId)).ToList();
            }
            catch (Exception ex)
            {

            }
            return new List<Contact>();
        }


        public Task<List<Contact>> GetContactsByLastName(string lastName)
        {
            throw new NotImplementedException();
        }

        public async Task<Contact> GetContactByEmail(string email)
        {
            var kslist = await _graph.GetKnowledgeStatesByTypeAndAttribute(_config["AppSettings:boaiuserid"], personObjectId, backofficeKG, emailLineage, email);
            return kslist.Select(a => ConvertContact(a, personObjectId)).FirstOrDefault();
        }

        public Task<Contact> GetContactById(string Id)
        {
            throw new NotImplementedException();
        }

        public async Task<Contact> UpdateContactAsync(Contact contact)
        {
            var ks = await _graph.GetKnowledgeStateByTypeAndAttribute(_config["AppSettings:boaiuserid"], personObjectId, backofficeKG, emailLineage, contact.Email);
            if (ks != null)
            {
                //update, remembering this may be a user, so overwrite with caution.
                UpdateAttribute(ks, personObjectId, firstNameLineage, "first name", contact.FirstName);
                UpdateAttribute(ks, personObjectId, lastNameLineage, "last name", contact.LastName);
                UpdateAttribute(ks, personObjectId, phoneLineage, "phone", contact.Phone);
                UpdateAttribute(ks, personObjectId, noteLineage, "notes", contact.Notes);
                UpdateAttribute(ks, personObjectId, occupationLineage, "occupation", contact.Title);
                UpdateAttribute(ks, personObjectId, companyLineage, "company", contact.Company);
                UpdateAttribute(ks, personObjectId, countryLineage, "country", contact.Country);
                UpdateAttribute(ks, personObjectId, sourceLineage, "source", contact.Source);
                UpdateAttribute(ks, personObjectId, sectorLineage, "sector", contact.Sector);
                //call update in db
                return contact;
            }
            await CreateContactAsync(contact);
            return contact;
        }


        public async Task<long> GetContactsCount(string userId)
        {
            var kslist = await _graph.GetKnowledgeStatesByType(_config["AppSettings:boaiuserid"], personObjectId, backofficeKG);
            return kslist.Select(a => Convert(a, personObjectId)).Count();
        }

        public Task<long> GetContactsDayCount(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<long> GetContactsMonthCount(string userId)
        {
            throw new NotImplementedException();
        }

        public async Task<Contact> CreateContactAsync(Contact contact)
        {
            var goi = new KnowledgeStateInput
            {
                subjectId = contact.Id,
                knowledgeGraphName = backofficeKG,
                data = new List<StringListGraphAttributeInputPair> {
                    new StringListGraphAttributeInputPair{ 
                        name = personObjectId, 
                        value = new List<GraphAttributeInput>{
                            new GraphAttributeInput {
                                name = "first name",
                                lineage = firstNameLineage,
                                type = GraphAttribute.DataType.textual,
                                value = contact.FirstName,
                                confidence = 1.0
                            },
                            new GraphAttributeInput {
                                name = "last name",
                                lineage = lastNameLineage,
                                type = GraphAttribute.DataType.textual,
                                value = contact.LastName,
                                confidence = 1.0
                            },
                            new GraphAttributeInput {
                                name = "email",
                                lineage = emailLineage,
                                type = GraphAttribute.DataType.textual,
                                value = contact.Email,
                                confidence = 1.0
                            },
                            new GraphAttributeInput {
                                name = "phone",
                                lineage = phoneLineage,
                                type = GraphAttribute.DataType.textual,
                                value = contact.Phone,
                                confidence = 1.0
                            },
                            new GraphAttributeInput {
                                name = "occupation",
                                lineage = occupationLineage,
                                type = GraphAttribute.DataType.textual,
                                value = contact.Title
                            },
                            new GraphAttributeInput {
                                name = "notes",
                                lineage = noteLineage,
                                type = GraphAttribute.DataType.textual,
                                value = contact.Notes,
                                confidence = 1.0
                            },
                            new GraphAttributeInput {
                                name = "company",
                                lineage = companyLineage,
                                type = GraphAttribute.DataType.textual,
                                value = contact.Company,
                                confidence = 1.0
                            },
                            new GraphAttributeInput {
                                name = "country",
                                lineage = countryLineage,
                                type = GraphAttribute.DataType.textual,
                                value = contact.Country,
                                confidence = 1.0
                            },
                            new GraphAttributeInput {
                                name = "source",
                                lineage = sourceLineage,
                                type = GraphAttribute.DataType.textual,
                                value = contact.Source,
                                confidence = 1.0
                            },
                            new GraphAttributeInput {
                                name = "sector",
                                lineage = sectorLineage,
                                type = GraphAttribute.DataType.textual,
                                value = contact.Sector ?? string.Empty,
                                confidence = 1.0
                            },
                            new GraphAttributeInput {
                                name = "existence",
                                lineage = existenceLineage,
                                type = GraphAttribute.DataType.temporal,
                                existence = new List<DarlTime?> { new DarlTime(contact.Created), DarlTime.MaxValue },
                                confidence = 1.0
                            }
                        }
                    }
                }
            };
            await _graph.CreateKnowledgeState(_config["AppSettings:boaiuserid"], goi);
            return contact; throw new NotImplementedException();
        }

        public async Task<Contact?> DeleteContactAsync(string email)
        {
            await _graph.DeleteKnowledgeState(_config["AppSettings:boaiuserid"], email, backofficeKG);
            return null;
        }


        #endregion

        #region Users

        public async Task<DarlUser> GetUserByApiKey(string apiKey)
        {
            var ks = await _graph.GetKnowledgeStateByTypeAndAttribute(_config["AppSettings:boaiuserid"], personObjectId, backofficeKG, keyLineage, apiKey);
            if (ks == null)
                return null;
            return Convert(ks, personObjectId);
        }

        public async Task<DarlUser> GetUserById(string id)
        {
            var ks = await _graph.GetKnowledgeState(_config["AppSettings:boaiuserid"], id, backofficeKG);
            if (ks == null)
                return null;
            return Convert(ks, personObjectId);
        }

        public async Task<List<DarlUser>> GetUsers()
        {
            try
            {
                var kslist = await _graph.GetKnowledgeStatesByTypeAndAttributeExistence(_config["AppSettings:boaiuserid"], personObjectId, backofficeKG,stateLineage);
                return kslist.Select(a => Convert(a, personObjectId)).ToList();
            }
            catch (Exception ex)
            {

            }
            return new List<DarlUser>();
        }

        public async Task<List<DarlUser>> GetRecentUsers()
        {
            DateTime cutOff = DateTime.UtcNow - new TimeSpan(7, 0, 0, 0);
            var kslist = await _graph.GetKnowledgeStatesByType(_config["AppSettings:boaiuserid"], personObjectId, backofficeKG);
            return kslist.Where(a => GetExistenceStart(a, personObjectId) > cutOff).Select(a => Convert(a, personObjectId)).ToList();
        }


        public async Task<DarlUser> GetUserByStripeId(string stripeId)
        {
            var ks = await _graph.GetKnowledgeStateByTypeAndAttribute(_config["AppSettings:boaiuserid"], personObjectId, backofficeKG, idLineage, stripeId);
            if (ks == null)
                return null;
            return Convert(ks, personObjectId);
        }

        public async Task<List<DarlUser>> GetUsersByEmail(string email)
        {
            var kslist = await _graph.GetKnowledgeStatesByTypeAndAttribute(_config["AppSettings:boaiuserid"], personObjectId, backofficeKG, emailLineage, email);
            return kslist.Select(a => Convert(a, personObjectId)).ToList();
        }

        public string GetCurrentUserId(object userContext)
        {
            if (userContext != null)
            {
                var ctxt = userContext as GraphQLUserContext;
                if (ctxt != null)
                    return ctxt.User.Identity.Name ?? _config["AppSettings:boaiuserid"];
            }
            return _config["AppSettings:boaiuserid"];
        }

        public async Task<DarlUser> UpdateUserAsync(string userId, DarlUserUpdate darlUserUpdate)
        {
            throw new NotImplementedException();
        }

        public async Task<string> UpdateUserAPIKey(string userId)
        {
            throw new NotImplementedException();
        }


        public Task<string> GetUserIdFromAppId(string appId)
        {
            throw new NotImplementedException();
        }

        public async Task<long> GetUserCount(string userId)
        {
            var kslist = await _graph.GetKnowledgeStatesByTypeAndAttributeExistence(_config["AppSettings:boaiuserid"], personObjectId, backofficeKG, stateLineage);
            return kslist.Count();
        }

        public async Task<DarlUser> CreateUserAsync(DarlUser user)
        {
            var names = string.IsNullOrEmpty(user.InvoiceName) ? new List<string>() : LineageLibrary.SimpleTokenizer(user.InvoiceName);
            var goi = new KnowledgeStateInput
            {
                subjectId = user.userId,
                knowledgeGraphName = backofficeKG,
                data = new List<StringListGraphAttributeInputPair> {
                    new StringListGraphAttributeInputPair
                    { 
                        name = personObjectId,
                        value = new List<GraphAttributeInput>
                        {
                             new GraphAttributeInput {
                                 name = "first name",
                                 lineage = firstNameLineage,
                                 type = GraphAttribute.DataType.textual,
                                 value = names.Any() ? names.First() : string.Empty,
                                 confidence = 1.0
                            },
                            new GraphAttributeInput {
                                name = "last name",
                                lineage = lastNameLineage,
                                type = GraphAttribute.DataType.textual,
                                value = names.Any() && names.Count > 1 ? names.Last() : string.Empty,
                                confidence = 1.0
                            },
                            new GraphAttributeInput {
                                name = "email",
                                lineage = emailLineage,
                                type = GraphAttribute.DataType.textual,
                                value = user.InvoiceEmail,
                                confidence = 1.0
                            },
                            new GraphAttributeInput {
                                name = "apiKey",
                                lineage = keyLineage,
                                type = GraphAttribute.DataType.textual,
                                value = user.APIKey,
                                confidence = 1.0
                            },
                            new GraphAttributeInput {
                                name = "stripe product",
                                lineage = subscriptionLineage,
                                type = GraphAttribute.DataType.textual,
                                value = user.productId,
                                confidence = 1.0
                            },
                            new GraphAttributeInput {
                                name = "Stripe id",
                                lineage = idLineage,
                                type = GraphAttribute.DataType.textual,
                                value = user.StripeCustomerId,
                                confidence = 1.0
                            },
                            new GraphAttributeInput {
                                name = "account state",
                                lineage = stateLineage,
                                type = GraphAttribute.DataType.categorical,
                                value = user.accountState.ToString(),
                                confidence = 1.0
                            },
                            new GraphAttributeInput {
                                name = "company",
                                lineage = companyLineage,
                                type = GraphAttribute.DataType.textual,
                                value = user.InvoiceOrganization ?? string.Empty,
                                confidence = 1.0
                            },
                            new GraphAttributeInput {
                                name = "existence",
                                lineage = existenceLineage,
                                type = GraphAttribute.DataType.temporal,
                                existence = new List<DarlTime?> { new DarlTime(user.Created), DarlTime.MaxValue },
                                value = string.Empty,
                                confidence = 1.0
                            }
                        }
                    }
                }
            };
            await _graph.CreateKnowledgeState(_config["AppSettings:boaiuserid"], goi);
            return new DarlUser();
        }

        public async Task<DarlUser.AccountState?> GetUserAccountState(string customerId)
        {
            var ks = await _graph.GetKnowledgeStateByTypeAndAttribute(_config["AppSettings:boaiuserid"], personObjectId, backofficeKG, idLineage, customerId);
            var stateString = GetAttributeValue(ks, personObjectId, stateLineage);
            if (!string.IsNullOrEmpty(stateString))
            {
                if (Enum.TryParse<DarlUser.AccountState>(stateString, out DarlUser.AccountState state))
                {
                    return state;
                }
            }
            return null;
        }

        public async Task UpdateUserAccountState(string customerId, DarlUser.AccountState paying)
        {
            var state = await GetUserAccountState(customerId);
            if(state != null)
            {
                if(state != DarlUser.AccountState.admin) //can't downgrade admin
                {
                   // UpdateAttribute()
                }
            }
        }


        #endregion

        #region Subscriptions

        public async Task<DarlUser> CreateAndRegisterNewUser(DarlUserInput user)
        {
            //Create stripe customer and internal user
            //productId in user is actually priceId, lookup
            var customerId = await CreateStripeCustomer(user.userId, user.InvoiceEmail, user.InvoiceName);
            var product = _prods.products.FirstOrDefault(a => a.priceId == user.productId);
            return await CreateUserAsync(new DarlUser { userId = user.userId, InvoiceName = user.InvoiceName, InvoiceEmail = user.InvoiceEmail, Created = DateTime.UtcNow, StripeCustomerId = customerId, APIKey = Guid.NewGuid().ToString(), accountState = DarlUser.AccountState.trial, productId = product == null ? string.Empty : product.id });
        }


        #endregion

        #region Licensing


        public Task<string> CreateKey(string userId, string company, string email, DateTime endDate)
        {
            throw new NotImplementedException();
        }

        public Task<bool> CheckKey(string userId, string key)
        {
            throw new NotImplementedException();
        }

        #endregion


        public async Task StoreSystemKG()
        {
            await _graph.Store(backofficeKGComp);
        }

        public async Task<bool> CreateNewGraph(string userId, string modelName)
        {
            var user = await GetUserById(userId);
            if (user == null)
                return false;
            var prod = _prods.products.FirstOrDefault(a => a.id == user.productId);
            if (prod == null)
                return false;
            var count = await _graph.GetKGraphCountAsync(userId);
            if (count >= prod.kgCount)
            {
                throw new ExecutionError($"Your subscription only permits {prod.kgCount} knowledge graphs. Upgrade to get more.");
            }
            return await _graph.CreateNewGraph(userId, modelName);
        }

        public async Task<string> ShareKGraph(string userId, string name, string sharerId, bool readOnly, bool hidden)
        {
            var otherUser = await GetUserById(sharerId);
            if (otherUser == null && sharerId != _config["AppSettings:boaiuserid"])
                return null;
            return await _graph.ShareKGraph(userId, name, sharerId, readOnly, hidden);
        }

        public async Task<string> RegisterForMarketing(string name, string email)
        {
            if(await CheckEmail(email))
            {
                try
                {
                    if (await GetContactByEmail(email) != null)
                        return "You're already in our system.";
                    await CreateContactAsync(new Contact {Id = Guid.NewGuid().ToString(), Created = DateTime.UtcNow, Source= "newsletter", Email = email, FirstName = name });
                    return "Thanks for signing up. You'll now receive our newsletters.";
                }
                catch(Exception ex)
                {
                    return "there was an error creating your contact record. Please try again later.";
                }
            }
            return "Your email is invalid according to ZeroBounce, please give us a valid one.";
        }

        public async Task<bool> CheckEmail(string email, string ipaddress = "")
        {
            return await _checkEmail.CheckEmail(email, ipaddress);
        }

        public async Task<string> GetSuggestedRuleSet(string userId, string modelName, string objectId, string lineage)
        {
            var model = await _graph.GetModel(userId, modelName);
            if (model != null)
            {
                return _meta.GetSuggestedRuleSet(model, objectId, lineage);
            }
            return string.Empty;
        }

        #region private

        private DarlUser Convert(KnowledgeState ks, string objectId)
        {
            return new DarlUser
            {
                accountState = ConvertAccountState(ks),
                userId = ks.subjectId,
                InvoiceEmail = GetAttributeValue(ks, objectId, emailLineage),
                InvoiceName = ($"{GetAttributeValue(ks, objectId, firstNameLineage)} {GetAttributeValue(ks, objectId, lastNameLineage)}").Trim(),
                InvoiceOrganization = GetAttributeValue(ks, objectId, companyLineage),
                APIKey = GetAttributeValue(ks, objectId, keyLineage),
                productId = GetAttributeValue(ks, objectId, subscriptionLineage),
                StripeCustomerId = GetAttributeValue(ks, objectId, idLineage),
            };
        }


        private string GetAttributeValue(KnowledgeState ks, string objectId, string lineage)
        {
            var att = ks.GetAttribute(objectId, lineage);
            if (att != null)
            {
                return att.value ?? string.Empty;
            }
            return string.Empty;
        }

        private List<DarlTime?> GetExistence(KnowledgeState ks, string objectId)
        {
            var att = ks.GetAttribute(objectId, existenceLineage);
            if (att != null)
            {
                return att.existence;
            }
            return new List<DarlTime?>();
        }

        private DateTime GetExistenceStart(KnowledgeState ks, string objectId)
        {
            var att = ks.GetAttribute(objectId, existenceLineage);
            if (att != null)
            {
                if(att.existence != null)
                {
                    var first = att.existence.First();
                    if(first != null)
                    {
                        return first.dateTime;
                    }
                }
            }
            return DateTime.MinValue;
        }

        private DarlUser.AccountState? ConvertAccountState(KnowledgeState ks)
        {
            var att = ks.GetAttribute(personObjectId, stateLineage);
            if (att == null || string.IsNullOrEmpty(att.value))
                return null;
            return (DarlUser.AccountState)Enum.Parse(typeof(DarlUser.AccountState), att.value);
        }

        private void UpdateAttribute(KnowledgeState ks, string objectId, string attlineage, string name, string value, bool overwrite = false)
        {
            if (string.IsNullOrEmpty(value))
                return;
            if (!ks.data.ContainsKey(objectId))
                return;
            var properties = ks.data[objectId];
            var val = properties.FirstOrDefault(a => a.lineage.StartsWith(attlineage));
            if (val == null)
            {
                properties.Add(new GraphAttribute
                {
                    id = Guid.NewGuid().ToString(),
                    name = name,
                    lineage = attlineage,
                    type = GraphAttribute.DataType.textual,
                    value = value,
                    confidence = 1.0
                });
            }
            else if (string.IsNullOrEmpty(val.value) || overwrite)
            {
                val.value = value;
            }
        }

        private void UpdateExistence(KnowledgeState ks, string objectId, List<DarlTime?> existence)
        {
            if (existence == null || !existence.Any())
                return;
            if (!ks.data.ContainsKey(objectId))
                return;
            var properties = ks.data[objectId];
            var val = properties.FirstOrDefault(a => a.lineage.StartsWith(existenceLineage));
            if (val == null)
            {
                properties.Add(new GraphAttribute
                {
                    id = Guid.NewGuid().ToString(),
                    name = "existence",
                    lineage = existenceLineage,
                    type = GraphAttribute.DataType.temporal,
                    existence = existence,
                    confidence = 1.0
                });
            }
            else
            {
                val.existence = existence;
            }
        }

        private Collateral ConvertCollateral(KnowledgeState ks, string objectId)
        {
            return new Collateral
            {
                Name = ks.subjectId,
                Value = GetAttributeValue(ks, objectId, valueLineage)
            };
        }

        private Contact ConvertContact(KnowledgeState ks, string personObjectId)
        {
            return new Contact
            {
                Id = ks.userId,
                Email = GetAttributeValue(ks, personObjectId, emailLineage),
                FirstName = GetAttributeValue(ks, personObjectId, firstNameLineage),
                LastName = GetAttributeValue(ks, personObjectId, lastNameLineage),
                Company = GetAttributeValue(ks, personObjectId, companyLineage),
                Title = GetAttributeValue(ks, personObjectId, occupationLineage),
                Notes = GetAttributeValue(ks, personObjectId, noteLineage),
                Country = GetAttributeValue(ks, personObjectId, countryLineage),
                Sector = GetAttributeValue(ks, personObjectId, sectorLineage),
                Phone = GetAttributeValue(ks, personObjectId, phoneLineage),
                Source = GetAttributeValue(ks, personObjectId, sourceLineage),
                Created = GetExistenceStart(ks, personObjectId)
            };
        }

        private async Task<string> CreateStripeCustomer(string userId, string email, string name = "")
        {
            var sak = _config["AppSettings:StripeAPIKey"];
            if (!string.IsNullOrEmpty(sak))
            {
                StripeConfiguration.ApiKey = sak;
                try
                {
                    var options = new CustomerCreateOptions
                    {
                        Email = email,
                        Description = userId,
                        Metadata = new Dictionary<string, string> { { nameof(userId), userId }, { nameof(name), name } }
                    };
                    var service = new CustomerService();
                    Customer customer = await service.CreateAsync(options);
                    return (customer.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, nameof(CreateStripeCustomer));
                }
            }
            return string.Empty;
        }

        #endregion
    }
}
