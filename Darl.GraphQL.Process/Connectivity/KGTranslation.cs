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

        public KGTranslation(ILogger<KGTranslation> logger, IConfiguration config, IGraphProcessing graph, IMetaStructureHandler meta, IProducts prods)
        {
            _config = config;
            _logger = logger;
            _graph = graph;
            _meta = meta;
            _prods = prods;
            backofficeKG = _config["AppSettings:BackOfficeKG"];
            backofficeKGComp = _config["AppSettings:boaiuserid"] + '_' + backofficeKG;
        }

        #region SystemUpdates

        public async Task<List<Update>> Updates()
        {
            var list = new List<Update>();
            var obs = await _graph.GetGraphObjectsByLineage(backofficeKGComp, processLineage);
            foreach(var o in obs)
            {
                var from = o.properties.FirstOrDefault(a => a.lineage.StartsWith(sourceLineage));
                var to = o.properties.FirstOrDefault(a => a.lineage.StartsWith(destinationLineage));
                if(from != null && to != null)
                {
                    var update = new Update { from = from.value, to = to.value, updated = o.existence.Last() != null ? o.existence.Last().dateTime : DateTime.MinValue };
                    list.Add(update);
                }
            }
            return list;
        }

        public async Task<DateTime> GetLastUpdate(string from, string to)
        {
            var obj = await GetUpdate(from, to);
            if(obj != null)
            {
                return obj.existence.Last() != null ? obj.existence.Last().dateTime : DateTime.MinValue;
            }
            return DateTime.MinValue;
        }

        public async Task<DateTime> SetLastUpdate(string from, string to)
        {
            var obj = await GetUpdate(from, to);
            if(obj != null)
            {
                obj.existence.Clear();
                obj.existence.Add(new DarlTime(DateTime.UtcNow));
            }
            else
            {
                var goi = new GraphObjectInput { 
                    name = $"{from} - {to}", 
                    lineage = processLineage,
                    externalId = $"{from} - {to}",
                    properties = new List<GraphAttribute> { 
                        new GraphAttribute {
                            name = "from", 
                            lineage = sourceLineage, 
                            type = GraphAttribute.DataType.textual,
                            value = from
                        }, new GraphAttribute {
                            name = "to",
                            lineage = destinationLineage,
                            type = GraphAttribute.DataType.textual,
                            value = to
                        } },
                    existence = new List<DarlTime?> { new DarlTime(DateTime.UtcNow) }
                };
                await _graph.CreateGraphObject(backofficeKGComp, goi, OntologyAction.build);
            }
            await _graph.Store(backofficeKGComp);
            return DateTime.UtcNow;
        }

        private async Task<GraphObject?> GetUpdate(string from, string to)
        {
            var obs = await _graph.GetGraphObjectsByLineage(backofficeKGComp, processLineage);
            foreach (var o in obs)
            {
                var tfrom = o.properties.FirstOrDefault(a => a.lineage.StartsWith(sourceLineage));
                var tto = o.properties.FirstOrDefault(a => a.lineage.StartsWith(destinationLineage));
                if (tfrom != null && tto != null)
                {
                    if (tfrom.value == from && tto.value == to)
                        return o;
                }
            }
            return null;
        }

        #endregion

        #region Defaults
        public async Task<List<Default>> GetDefaults()
        {
            var list = new List<Default>();
            var obs = await _graph.GetGraphObjectsByLineage(backofficeKGComp, defaultLineage);
            foreach (var o in obs)
            {
                var val = o.properties.FirstOrDefault(a => a.lineage.StartsWith(valueLineage));
                if (val != null )
                {
                    var def = new Default { Name = o.externalId, Value = val.value };
                    list.Add(def);
                }
            }
            return list;
        }

        public async Task<string> GetDefaultValue(string name)
        {
            var o = await _graph.GetGraphObjectByExternalId(backofficeKGComp, name);
            var val = o.properties.FirstOrDefault(a => a.lineage.StartsWith(valueLineage));
            if (val != null)
            {
                return val.value;
            }
            return string.Empty;
        }

        public async Task<Default> CreateDefault(string name, string value)
        {
            var goi = new GraphObjectInput
            {
                name = name,
                lineage = defaultLineage,
                externalId = name,
                properties = new List<GraphAttribute> {
                        new GraphAttribute {
                            name = "value",
                            lineage = valueLineage,
                            type = GraphAttribute.DataType.textual,
                            value = value
                        } }
            };
            await _graph.CreateGraphObject(backofficeKGComp, goi, OntologyAction.build);
            await _graph.Store(backofficeKGComp);
            return new Default { Name = name, Value = value };
        }

        public async Task<Default?> DeleteDefault(string name)
        {
            var o = await _graph.GetGraphObjectByExternalId(backofficeKGComp, name);
            if (o != null)
            {
                await _graph.DeleteGraphObject(backofficeKGComp, o.id);
                var val = o.properties.FirstOrDefault(a => a.lineage.StartsWith(valueLineage));
                if (val != null)
                {
                    await _graph.Store(backofficeKGComp);
                    return new Default { Name = name, Value = val.value };
                }
            }
            return null;
        }

        public async Task<Default> UpdateDefault(string name, string value)
        {
            var o = await _graph.GetGraphObjectByExternalId(backofficeKGComp, name);
            if (o != null)
            {
                var val = o.properties.FirstOrDefault(a => a.lineage.StartsWith(valueLineage));
                if (val != null)
                {
                    val.value = value;
                    await _graph.Store(backofficeKGComp);
                    return new Default { Name = name, Value = val.value };
                }
            }
            return await CreateDefault(name, value);
        }

        #endregion

        #region Collateral

        public async Task<string> GetCollateral(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                var o = await _graph.GetGraphObjectByExternalId(backofficeKGComp, name);
                if (o != null && o.properties != null)
                {
                    var val = o.properties.FirstOrDefault(a => a.lineage.StartsWith(valueLineage));
                    if (val != null)
                    {
                        return val.value;
                    }
                }
            }
            return string.Empty;
        }

        public async Task<Collateral> UpdateCollateral(string name, string value)
        {
            var o = await _graph.GetGraphObjectByExternalId(backofficeKGComp, name);
            if (o != null)
            {
                var val = o.properties.FirstOrDefault(a => a.lineage.StartsWith(valueLineage));
                if (val != null)
                {
                    val.value = value;
                    await _graph.Store(backofficeKGComp);
                    return new Collateral { Name = name, Value = val.value };
                }
            }
            var goi = new GraphObjectInput
            {
                name = name,
                lineage = collateralLineage,
                externalId = name,
                properties = new List<GraphAttribute> {
                        new GraphAttribute {
                            name = "value",
                            lineage = valueLineage,
                            type = GraphAttribute.DataType.textual,
                            value = value
                        } }
            };
            await _graph.CreateGraphObject(backofficeKGComp, goi, OntologyAction.build);
            await _graph.Store(backofficeKGComp);
            return new Collateral { Name = name, Value = value };
        }

        public async Task<Collateral?> DeleteCollateral(string name)
        {
            var o = await _graph.GetGraphObjectByExternalId(backofficeKGComp, name);
            if (o != null)
            {
                await _graph.DeleteGraphObject(backofficeKGComp, o.id);
                var val = o.properties.FirstOrDefault(a => a.lineage.StartsWith(valueLineage));
                if (val != null)
                {
                    await _graph.Store(backofficeKGComp);
                    return new Collateral { Name = name, Value = val.value };
                }
            }
            return null;
        }

        public async Task<List<Collateral>> GetCollaterals()
        {
            var list = new List<Collateral>();
            var obs = await _graph.GetGraphObjectsByLineage(backofficeKGComp, collateralLineage);
            foreach (var o in obs)
            {
                var val = o.properties.FirstOrDefault(a => a.lineage.StartsWith(valueLineage));
                if (val != null)
                {
                    var def = new Collateral { Name = o.externalId, Value = val.value };
                    list.Add(def);
                }
            }
            return list;
        }

        #endregion

        #region Contacts

        public Task<List<Contact>> GetRecentContacts()
        {
            throw new NotImplementedException();
        }

        public IQueryable<Contact> GetContactsQueryable()
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

        public Task<Contact> GetContactByEmail(string email)
        {
            throw new NotImplementedException();
        }

        public Task<Contact> GetContactById(string Id)
        {
            throw new NotImplementedException();
        }

        public async Task<Contact> UpdateContactAsync(Contact contact)
        {
            var o = await _graph.GetGraphObjectByExternalId(backofficeKGComp, contact.Email);
            if (o != null)
            {
                //update, remembering this may be a user, so overwrite with caution.
                UpdateAttribute(o, firstNameLineage, "first name", contact.FirstName);
                UpdateAttribute(o, lastNameLineage, "last name", contact.LastName);
                UpdateAttribute(o, phoneLineage, "phone", contact.Phone);
                UpdateAttribute(o, noteLineage, "notes", contact.Notes);
                UpdateAttribute(o, occupationLineage, "occupation", contact.Title);
                UpdateAttribute(o, companyLineage, "company", contact.Company);
                UpdateAttribute(o, countryLineage, "country", contact.Country);
                UpdateAttribute(o, sourceLineage, "source", contact.Source);
                UpdateAttribute(o, sectorLineage, "sector", contact.Sector);
            }
            var goi = new GraphObjectInput
            {
                name = contact.Email,
                lineage = _meta.CommonLineages["person"],
                externalId = Guid.NewGuid().ToString(),
                existence = new List<DarlTime?> { new DarlTime(contact.Created), DarlTime.MaxValue },
                properties = new List<GraphAttribute> {
                        new GraphAttribute {
                            name = "first name",
                            lineage = firstNameLineage,
                            type = GraphAttribute.DataType.textual,
                            value = contact.FirstName,
                            confidence = 1.0
                        },
                        new GraphAttribute {
                            name = "last name",
                            lineage = lastNameLineage,
                            type = GraphAttribute.DataType.textual,
                            value = contact.LastName,
                            confidence = 1.0
                        },
                        new GraphAttribute {

                            name = "email",
                            lineage = emailLineage,
                            type = GraphAttribute.DataType.textual,
                            value = contact.Email,
                            confidence = 1.0
                        },
                        new GraphAttribute {
                            name = "phone",
                            lineage = phoneLineage,
                            type = GraphAttribute.DataType.textual,
                            value = contact.Phone,
                            confidence = 1.0
                        },
                        new GraphAttribute {
                            name = "occupation",
                            lineage = occupationLineage,
                            type = GraphAttribute.DataType.textual,
                            value = contact.Title
                        },
                        new GraphAttribute {
                            name = "notes",
                            lineage = noteLineage,
                            type = GraphAttribute.DataType.textual,
                            value = contact.Notes,
                            confidence = 1.0
                        },
                        new GraphAttribute {
                            name = "company",
                            lineage = companyLineage,
                            type = GraphAttribute.DataType.textual,
                            value = contact.Company,
                            confidence = 1.0
                        },
                        new GraphAttribute {
                            name = "country",
                            lineage = countryLineage,
                            type = GraphAttribute.DataType.textual,
                            value = contact.Country,
                            confidence = 1.0
                        },
                        new GraphAttribute {
                            name = "source",
                            lineage = sourceLineage,
                            type = GraphAttribute.DataType.textual,
                            value = contact.Source,
                            confidence = 1.0
                        },
                        new GraphAttribute {
                            name = "sector",
                            lineage = sectorLineage,
                            type = GraphAttribute.DataType.textual,
                            value = contact.Sector,
                            confidence = 1.0
                        }
                }
            };
            await _graph.CreateGraphObject(backofficeKGComp, goi, OntologyAction.build);
 //           await _graph.Store(backofficeKG);
            return contact;
        }

        private void UpdateAttribute(GraphObject o, string attlineage, string name, string value)
        {
            if (string.IsNullOrEmpty(value))
                return;
            var val = o.properties.FirstOrDefault(a => a.lineage.StartsWith(attlineage));
            if (val == null)
            {
                o.properties.Add(new GraphAttribute
                {
                    name = name,
                    lineage = attlineage,
                    type = GraphAttribute.DataType.textual,
                    value = value,
                    confidence = 1.0
                });
            }
            else if(string.IsNullOrEmpty(val.value))
            {
                val.value = value;
            }
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

        #endregion

        #region Users

        public async Task<DarlUser> GetUserByApiKey(string apiKey)
        {
            var m = await _graph.GetModel(_config["AppSettings:boaiuserid"], backofficeKG);
            return m.vertices.Where(a => a.Value.lineage.StartsWith(_meta.CommonLineages["person"]) && (a.Value.GetAttributeValue(keyLineage) ?? "") == apiKey).Select(a => Convert(a.Value)).FirstOrDefault();
        }

        public async Task<DarlUser> GetUserById(string id)
        {
            var m = await _graph.GetModel(_config["AppSettings:boaiuserid"], backofficeKG);
            return m.vertices.Where(a =>  a.Value.externalId == id).Select(a => Convert(a.Value)).FirstOrDefault();
        }

        public async Task<List<DarlUser>> GetUsers()
        {
            try
            {
                var m = await _graph.GetModel(_config["AppSettings:boaiuserid"], backofficeKG);
                return m.vertices.Where(a => a.Value.GetAttributeValue(stateLineage) != null).Select(a => Convert(a.Value)).ToList();
            }
            catch(Exception ex)
            {

            }
            return new List<DarlUser>();
        }

        private DarlUser Convert(GraphObject o)
        {
            return new DarlUser
            {
                accountState = ConvertAccountState(o),
                userId = o.externalId,
                InvoiceEmail = o.GetAttributeValue(emailLineage),
                InvoiceName = ($"{o.GetAttributeValue(firstNameLineage)} {o.GetAttributeValue(lastNameLineage)}").Trim(),
                InvoiceOrganization = o.GetAttributeValue(companyLineage),
                APIKey = o.GetAttributeValue(keyLineage),
                productId = o.GetAttributeValue(subscriptionLineage),
                StripeCustomerId = o.GetAttributeValue(idLineage),
            };
        }

        private static DarlUser.AccountState? ConvertAccountState(GraphObject o)
        {
            var att = o.GetAttributeValue(stateLineage);
            if (string.IsNullOrEmpty(att))
                return null;
            return (DarlUser.AccountState)Enum.Parse(typeof(DarlUser.AccountState),att);
        }

        private static DarlUser.SubscriptionType? ConvertSubscriptionType(GraphObject o)
        {
            var att = o.GetAttributeValue(typeLineage);
            if (string.IsNullOrEmpty(att))
                return null;
            return (DarlUser.SubscriptionType)Enum.Parse(typeof(DarlUser.SubscriptionType), att);
        }

        public async Task<List<DarlUser>> GetUsersByEmail(string email)
        {
            var m = await _graph.GetModel(_config["AppSettings:boaiuserid"], backofficeKG);
            return m.vertices.Where(a => a.Value.ContainsAttribute(stateLineage) && (a.Value.GetAttributeValue(emailLineage) ?? "") == email).Select(a => Convert(a.Value)).ToList();
        }

        public string GetCurrentUserId(object userContext)
        {
            if (userContext != null)
            {
                var ctxt = userContext as GraphQLUserContext;
                if(ctxt != null)
                    return ctxt.User.Identity.Name ?? _config["AppSettings:boaiuserid"];
            }
            return _config["AppSettings:boaiuserid"];
        }

        public Task<DarlUser> UpdateUserAsync(string userId, DarlUserUpdate darlUserUpdate)
        {
            throw new NotImplementedException();
        }

        public async Task<string> UpdateUserAPIKey(string userId)
        {
            var m = await _graph.GetModel(_config["AppSettings:boaiuserid"], backofficeKG);
            var o =  m.vertices.Where(a => a.Value.id == userId).FirstOrDefault().Value;
            if(o != null)
            {
                var newKey = Guid.NewGuid().ToString();
                UpdateAttribute(o, keyLineage, "apiKey", newKey);
                return newKey;
            }
            return string.Empty;
        }

        public async Task<DarlUser> GetUserByStripeId(string stripeId)
        {
            var m = await _graph.GetModel(_config["AppSettings:boaiuserid"], backofficeKG);
            return m.vertices.Where(a => a.Value.GetAttributeValue(idLineage) == stripeId).Select(a => Convert(a.Value)).FirstOrDefault();
        }

        public Task<string> GetUserIdFromAppId(string appId)
        {
            throw new NotImplementedException();
        }

        public async Task<long> GetUserCount(string userId)
        {
            var m = await _graph.GetModel(_config["AppSettings:boaiuserid"], backofficeKG);
            return m.vertices.Where(a => a.Value.GetAttributeValue(stateLineage) != null).Count();
        }

        public async Task<DarlUser> CreateUserAsync(DarlUser user)
        {
            var names = string.IsNullOrEmpty(user.InvoiceName) ? new List<string>() : LineageLibrary.SimpleTokenizer(user.InvoiceName);
            var goi = new GraphObjectInput
            {
                name = user.InvoiceEmail,
                lineage = _meta.CommonLineages["person"],
                externalId = user.userId,
                existence = new List<DarlTime?> { new DarlTime(user.Created), DarlTime.MaxValue },
                properties = new List<GraphAttribute> {
                         new GraphAttribute {
                            name = "first name",
                            lineage = firstNameLineage,
                            type = GraphAttribute.DataType.textual,
                            value = names.Any() ? names.First() : string.Empty,
                            confidence = 1.0
                        },
                        new GraphAttribute {
                            name = "last name",
                            lineage = lastNameLineage,
                            type = GraphAttribute.DataType.textual,
                            value = names.Any() && names.Count > 1 ? names.Last() : string.Empty,
                            confidence = 1.0
                        },
                        new GraphAttribute {
                            name = "email",
                            lineage = emailLineage,
                            type = GraphAttribute.DataType.textual,
                            value = user.InvoiceEmail,
                            confidence = 1.0
                        },
                        new GraphAttribute {
                            name = "apiKey",
                            lineage = keyLineage,
                            type = GraphAttribute.DataType.textual,
                            value = user.APIKey,
                            confidence = 1.0
                        },
                        new GraphAttribute {
                            name = "stripe subscription",
                            lineage = subscriptionLineage,
                            type = GraphAttribute.DataType.textual,
                            value = user.productId,
                            confidence = 1.0
                        },
                        new GraphAttribute {
                            name = "Stripe id",
                            lineage = idLineage,
                            type = GraphAttribute.DataType.textual,
                            value = user.StripeCustomerId,
                            confidence = 1.0
                        },
                        new GraphAttribute {
                            name = "account state",
                            lineage = stateLineage,
                            type = GraphAttribute.DataType.categorical,
                            value = user.accountState.ToString(),
                            confidence = 1.0
                        },
                        new GraphAttribute {
                            name = "company",
                            lineage = companyLineage,
                            type = GraphAttribute.DataType.textual,
                            value = user.InvoiceOrganization,
                            confidence = 1.0
                        }           
                }
            };
            await _graph.CreateGraphObject(backofficeKGComp, goi, OntologyAction.build);
            await _graph.Store(backofficeKGComp);
            return new DarlUser();
        }

        #endregion

        #region Subscriptions

        public async Task<DarlUser> CreateAndRegisterNewUser(DarlUserInput user)
        {
            //Create stripe customer and internal user
            var customerId = await CreateStripeCustomer(user.userId, user.InvoiceEmail, user.InvoiceName);
            return await CreateUserAsync(new DarlUser { userId = user.userId, InvoiceName = user.InvoiceName, InvoiceEmail = user.InvoiceEmail, Created = DateTime.UtcNow, StripeCustomerId = customerId, APIKey = Guid.NewGuid().ToString(), accountState = DarlUser.AccountState.trial, productId = user.productId });
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
                        Metadata = new Dictionary<string, string> { { nameof(userId), userId }, { nameof(name), name }}
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

        public async Task UpdateUserAccountState(string customerId, DarlUser.AccountState paying)
        {
            var m = await _graph.GetModel(_config["AppSettings:boaiuserid"], backofficeKG);
            var user =  m.vertices.Where(a => a.Value.lineage.StartsWith(_meta.CommonLineages["person"]) && (a.Value.GetAttributeValue(idLineage) ?? "") == customerId).Select(a => a.Value).FirstOrDefault();
            if(user != null)
            {
                UpdateAttribute(user, stateLineage, "account state", paying.ToString());
            }
        }

        public async Task<bool> SendEmail(string email, string name, string subjectDefault, string contentNameDefault)
        {
            try
            {
/*                var test = _config.GetValue<bool>("StripeTest");
                var subject = await GetDefault(subjectDefault);
                var contentName = await GetDefault(contentNameDefault);
                var addressFrom = await GetDefault("SendStatusChange.addressFrom");
                var content = await GetMailContent(_config.GetValue<string>("adminuserid"), contentName);
                var pars = new Dictionary<string, string>();
                pars.Add(nameof(ind.invoiceName), string.IsNullOrEmpty(ind.invoiceName) ? "DARL user" : ind.invoiceName);
                pars.Add(nameof(ind.invoiceOrganization), string.IsNullOrEmpty(ind.invoiceOrganization) ? "Your organization" : ind.invoiceOrganization);
                var t = new TextProcess();
                var insertedContent = t.Parse(content, pars) as string;
                var to = test ? "test@darl.ai" : ind.invoiceEmail;
                //put into queue as List<string>
                QueueClient queueClient = new QueueClient(_config.GetValue<string>("StorageConnectionString"), "support-messages");
                await queueClient.CreateIfNotExistsAsync();
                await queueClient.SendMessageAsync(JsonConvert.SerializeObject(new SupportMailMessage { from = addressFrom, to = email, subject = subject, content = insertedContent }));*/
            }
            catch
            {
                return false;
            }
            return true; throw new NotImplementedException();
        }

        public async Task<DarlUser.AccountState?> GetUserAccountState(string customerId)
        {
            var m = await _graph.GetModel(_config["AppSettings:boaiuserid"], backofficeKG);
            var user = m.vertices.Where(a => a.Value.lineage.StartsWith(_meta.CommonLineages["person"]) && (a.Value.GetAttributeValue(idLineage) ?? "") == customerId).Select(a => a.Value).FirstOrDefault();
            var stateString = user.GetAttributeValue(stateLineage);
            if (!string.IsNullOrEmpty(stateString))
            {
                if (Enum.TryParse<DarlUser.AccountState>(stateString, out DarlUser.AccountState state))
                {
                    return state;
                }
            }
            return null;
        }

        public async Task<bool> CreateNewGraph(string userId, string modelName)
        {
            var user = await GetUserById(userId);
            if (user == null)
                return false;
            var prod = _prods.products.FirstOrDefault(a => a.priceId == user.productId);
            if (prod == null)
                return false;
            var count = await _graph.GetKGraphCountAsync(userId);
            if (count >= prod.kgCount)
            {
                throw new ExecutionError($"Your subscription only permits {prod.kgCount} knowledge graphs. Upgrade to get more.");
            }
            return await _graph.CreateNewGraph(userId, modelName);
        }
    }
}
