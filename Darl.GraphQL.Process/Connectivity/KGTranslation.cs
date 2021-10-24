using Darl.Common;
using Darl.GraphQL.Models.Middleware;
using Darl.GraphQL.Models.Models;
using Darl.GraphQL.Models.Models.Noda;
using Darl.GraphQL.Process.Models.Noda.Layout;
using Darl.Lineage;
using Darl.Thinkbase;
using Darl.Thinkbase.Meta;
using DarlCommon;
using DarlCompiler;
using GraphQL;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Connectivity
{
    /// <summary>
    /// Transforms between GraphQL processes and KGs
    /// </summary>
    public class KGTranslation : IKGTranslation
    {
        private readonly IConfiguration _config;
        private readonly ILogger<KGTranslation> _logger;
        private readonly IGraphProcessing _graph;
        private readonly IMetaStructureHandler _meta;
        private readonly IProducts _prods;
        private readonly ICheckEmail _checkEmail;
        private readonly ILicensing _licensing;
        private readonly DarlMetaRunTime metaRuntime = new DarlMetaRunTime(new MetaStructureHandler());


        //fill in with single call to model at startup
        private string personObjectId { get; set; } = string.Empty;
        private string defaultObjectId { get; set; } = string.Empty;
        private string collateralObjectId { get; set; } = string.Empty;
        private string updateObjectId { get; set; } = string.Empty;


        public static string backofficeKGComp = String.Empty;
        private static string backofficeKG = String.Empty;
        private static readonly string sourceLineage = "noun:01,4,04,02,21,16";
        private static readonly string destinationLineage = "noun:01,0,0,15,15,3";
        private static readonly string processLineage = "noun:00,4";
        private static readonly string defaultLineage = "noun:01,0,0,15,07,02,06,05";//constant
        private static readonly string valueLineage = "noun:01,4,04,02,07,01";//text
        private static readonly string collateralLineage = "noun:00,1,00,3,10,09,07";//document
        private static readonly string firstNameLineage = "noun:01,3,14,01,06,13";//first name
        private static readonly string lastNameLineage = "noun:01,3,14,01,06,11";//surname
        private static readonly string emailLineage = "noun:01,0,2,00,38,00,06,1";//email
        private static readonly string phoneLineage = "noun:01,4,07,01";//phone
        private static readonly string occupationLineage = "noun:01,0,2,00,23,19";//occupation
        private static readonly string noteLineage = "noun:01,4,05,21,28,1";//note
        private static readonly string companyLineage = "noun:01,2,07,10";//organization
        private static readonly string countryLineage = "noun:01,2,06,35";//nation
        private static readonly string sectorLineage = "noun:01,0,0,15,07,02,04,1,02,1";//sector
        private static readonly string keyLineage = "noun:01,4,09,01,7,3,0";//key
        private static readonly string subscriptionLineage = "noun:01,0,2,00,34,6,1,5,0";
        private static readonly string idLineage = "noun:01,4,09,01,7,3,5";
        private static readonly string stateLineage = "noun:01,1,00";
        private static readonly string typeLineage = "noun:01,0,0,15,07,02,02,0,01";
        private static readonly string existenceLineage = "noun:01,5,03,3,018";//life

        public KGTranslation(ILogger<KGTranslation> logger, IConfiguration config, IGraphProcessing graph, IMetaStructureHandler meta, IProducts prods, ICheckEmail checkEmail, ILicensing licensing)
        {
            _config = config;
            _logger = logger;
            _graph = graph;
            _meta = meta;
            _prods = prods;
            _checkEmail = checkEmail;
            _licensing = licensing;
            backofficeKG = _config["AppSettings:BackOfficeKG"];
            backofficeKGComp = _config["AppSettings:boaiuserid"] + '_' + backofficeKG;
            DarlMetaRunTime.SetLicense(_config["licensing:darlMetaLicense"]);
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
            if (ks != null)
                return GetAttributeValue(ks, defaultObjectId, valueLineage) ?? string.Empty;
            return string.Empty;
        }

        public async Task<Default> CreateDefault(string name, string value)
        {
            var goi = new KnowledgeStateInput
            {
                subjectId = name,
                knowledgeGraphName = backofficeKG,
                data = new List<StringListGraphAttributeInputPair>
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
                if (ks != null)
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
            catch (Exception)
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
            catch (Exception)
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
            try
            {
                var ks = await _graph.GetKnowledgeStateByTypeAndAttribute(_config["AppSettings:boaiuserid"], personObjectId, backofficeKG, keyLineage, apiKey);
                if (ks == null)
                    return null;
                return Convert(ks, personObjectId);
            }
            catch (Exception ex)
            {
                throw new ExecutionError($"Error in GetUserByApiKey: {ex.Message}");
            }
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
                var kslist = await _graph.GetKnowledgeStatesByTypeAndAttributeExistence(_config["AppSettings:boaiuserid"], personObjectId, backofficeKG, stateLineage);
                return kslist.Select(a => Convert(a, personObjectId)).ToList();
            }
            catch (Exception)
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
            if (state != null)
            {
                if (state != DarlUser.AccountState.admin) //can't downgrade admin
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
            return Task.FromResult(_licensing.CheckKey(key));
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
            if (await CheckEmail(email))
            {
                try
                {
                    if (await GetContactByEmail(email) != null)
                        return "You're already in our system.";
                    await CreateContactAsync(new Contact { Id = Guid.NewGuid().ToString(), Created = DateTime.UtcNow, Source = "newsletter", Email = email, FirstName = name });
                    return "Thanks for signing up. You'll now receive our newsletters.";
                }
                catch (Exception)
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

        /// <summary>
        /// create a Noda version of the graph and generate a trial 3D layout.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="graphName"></param>
        /// <returns></returns>
        public async Task<string> ExportNoda(string userId, string graphName)
        {
            //read model and convert to noda format
            var model = await _graph.GetModel(userId, graphName);
            //for coloring the nodes get the set of lineages.
            var lineageMap = new HashSet<string>();
            foreach (var k in model.vertices.Keys)
            {
                lineageMap.Add(model.vertices[k].lineage);
            }
            var colors = ColorGenerator.Pick(lineageMap.Count);
            if (!colors.Any()) //if only one lineage this will be empty
                colors.Add(new NodaTone { r = 0.0, g = 0.0, b = 1.0, a = 1.0 });
            int index = 0;
            var colourMap = new Dictionary<string, NodaTone>();
            foreach (var k in lineageMap)
            {
                colourMap.Add(k, colors[index++]);
            }
            var nodadoc = new NodaDocument { name = graphName };
            foreach (var k in model.vertices.Keys)
            {
                var tNode = model.vertices[k];
                var n = new NodaNode { title = tNode.name, uuid = k, properties = Convert(tNode.properties), tone = colourMap[tNode.lineage], size = 5.0, shape = NodaNodeShapes.Ball };
                nodadoc.nodes.Add(n);
            }
            foreach (var k in model.edges.Keys)
            {
                var tLink = model.edges[k];
                var l = new NodaLink { title = tLink.name, uuid = k, fromNode = new NodaNodeId { Uuid = tLink.startId }, toNode = new NodaNodeId { Uuid = tLink.endId }, properties = Convert(tLink.properties), tone = new NodaTone { r = 0.09019608, g = 0.09019608, b = 0.09019608, a = 1.0 } };
                nodadoc.links.Add(l);
            }
            //create the ancillary dictionaries
            nodadoc.Init();
            //run force layout on graph
            var fd = new ForceDirected3D(nodadoc, 81.76, 40000.0, 0.5);
            for (int n = 0; n < 100; n++)
                fd.Calculate(0.01);
            var bb = fd.GetBoundingBox();
            var diagonal = bb.topRightBack - bb.bottomLeftFront;
            var length = diagonal.Magnitude();
            var scale = 2.0 / length; //fit into a 2 unit diagonal bounding box
            foreach (var n in nodadoc.nodes)
            {
                n.position = n.position * scale;
            }
            return JsonConvert.SerializeObject(nodadoc, new Newtonsoft.Json.Converters.StringEnumConverter());
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
                catch (Exception)
                {

                }

            }
            return Task.FromResult(errorList);
        }

        public async Task<List<GraphAttribute>> GetConceptCloudData(string userId, string graphName, string address)
        {
            var list = new List<GraphAttribute>();
            if (!await _graph.Exists(userId, graphName))
                throw new ExecutionError($"{graphName} does not exist in this account");
            var graph = await _graph.GetModel(userId, graphName);
            if(string.IsNullOrEmpty(address))//root
            {//return the type words for the real objects derived from the virtual
                foreach (var g in graph.virtualVertices.Values.Where(a => a.In.Count == 0)) //All leaf virtual vertices
                {
                    list.Add(new GraphAttribute { value = (g.name ?? "").Replace('/','-'), name = "typeword", type = GraphAttribute.DataType.textual, lineage = g.lineage });
                }
            }
            else
            {
                var parts = address.Split('/');
            }
            return list;
        }


        #region private

        private List<NodaProperty> Convert(List<GraphAttribute> properties)
        {
            var list = new List<NodaProperty>();
            if (properties != null)
            {
                foreach (var a in properties)
                {
                    list.Add(Convert(a));
                }
            }
            return list;
        }

        private NodaProperty Convert(GraphAttribute att)
        {
            return new NodaProperty { name = att.name, text = string.IsNullOrEmpty(att.value) ? att.name : att.value, uuid = att.id, /*notes = JsonConvert.SerializeObject(att)*/ };
        }

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
                if (att.existence != null)
                {
                    var first = att.existence.First();
                    if (first != null)
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
