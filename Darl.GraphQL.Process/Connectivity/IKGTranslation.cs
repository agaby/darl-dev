using Darl.GraphQL.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Connectivity
{
    public interface IKGTranslation
    {
        Task<Default> CreateDefault(string name, string value);
        Task<Default?> DeleteDefault(string name);
        Task<List<Default>> GetDefaults();
        Task<string> GetDefaultValue(string name);
        Task<DateTime> GetLastUpdate(string from, string to);
        Task<DateTime> SetLastUpdate(string from, string to);
        Task<Default> UpdateDefault(string name, string value);
        Task<List<Update>> Updates();
        Task<string> GetCollateral(string name);
        Task<Collateral> UpdateCollateral(string name, string value);
        Task<Collateral?> DeleteCollateral(string name);
        Task<List<Collateral>> GetCollaterals();
        Task<List<Contact>> GetRecentContacts();
        Task<IQueryable<Contact>> GetContactsQueryable();
        Task<List<Contact>> GetContacts();
        Task<List<Contact>> GetContactsByLastName(string lastName);
        Task<Contact> GetContactByEmail(string email);
        Task<Contact> GetContactById(string Id);
        Task<Contact> UpdateContactAsync(Contact contact);
        Task<long> GetContactsCount(string userId);
        Task<long> GetContactsDayCount(string userId);
        Task<long> GetContactsMonthCount(string userId);
        Task<DarlUser> GetUserByApiKey(string apiKey);
        Task<DarlUser> GetUserById(string id);
        Task<List<DarlUser>> GetUsers();
        Task<List<DarlUser>> GetUsersByEmail(string email);
        Task<Contact> CreateContactAsync(Contact contact);
        string GetCurrentUserId(object userContext);
        Task<DarlUser> UpdateUserAsync(string userId, DarlUserUpdate darlUserUpdate);
        Task<string> UpdateUserAPIKey(string userId);
        Task<DarlUser> GetUserByStripeId(string stripeId);
        Task<string> GetUserIdFromAppId(string appId);
        Task<long> GetUserCount(string userId);
        Task<string> CreateKey(string userId, string company, string email, DateTime endDate);
        Task<bool> CheckKey(string userId, string key);
        Task<DarlUser> CreateUserAsync(DarlUser user);
        Task<DarlUser> CreateAndRegisterNewUser(DarlUserInput user);
        Task StoreSystemKG();
        Task UpdateUserAccountState(string id, DarlUser.AccountState paying);
        Task<bool> SendEmail(string email, string name, string v1, string v2);
        Task<DarlUser.AccountState?> GetUserAccountState(string customerId);
        Task<bool> CreateNewGraph(string userId, string modelName);
        Task<Contact> DeleteContactAsync(string emailname);
        Task<string> RegisterForMarketing(string name, string email);
        Task<bool> CheckEmail(string email, string ipaddress = "");
        Task<string> GetSuggestedRuleSet(string userId, string modelName, string objectId, string lineage);
    }
}
