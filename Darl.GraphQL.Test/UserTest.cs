using Darl.GraphQL.Models.Models;
using GraphQL.Client;
using GraphQL.Common.Request;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static Darl.GraphQL.Models.Models.DarlUser;

namespace Darl.GraphQL.Test
{
    [TestClass]
    public class UserTest
    {
        GraphQLClient client = null;


        [TestInitialize()]
        public void Initialize()
        {
            client = new GraphQLClient("https://darl.dev/graphql/");
            var authcode = "86f98f0e-0b42-409b-a304-44de4bd99113";
            client.DefaultRequestHeaders.Add("Authorization", $"Basic {authcode}");
            client.Options.JsonSerializerSettings.Converters.Add(new StringEnumConverter());
        }


        [TestCleanup()]
        public void Cleanup()
        {

        }

        [TestMethod]
        public async Task TestGetUser()
        {
            var IndexEmail = "andy@scientio.com";
            var req = new GraphQLRequest() { OperationName = "getUsersByEmail", Variables = new { email = IndexEmail }, Query = @"query getUsersByEmail($email: String!){  usersByEmail(email: $email){userId invoiceEmail}}" };
            var resp = await client.PostAsync(req);
            Assert.IsTrue(resp.Errors == null || resp.Errors.Length == 0);
            var users = resp.GetDataFieldAs<List<DarlUser>>("usersByEmail");
            req = new GraphQLRequest() { OperationName = "getUserById", Variables = new { userId = users[0].userId }, Query = @"query getUserById($userId: String!){  userById(userId: $userId){userId invoiceEmail}}" };
            resp = await client.PostAsync(req);
            Assert.IsTrue(resp.Errors == null || resp.Errors.Length == 0);
            var user = resp.GetDataFieldAs<DarlUser>("userById");
            Assert.AreEqual(IndexEmail, user.InvoiceEmail);

        }

        [TestMethod]
        public async Task TestSetUser()
        {
            var trialEmail = "andy@darl.dev";
            var userId = Guid.NewGuid().ToString();
            var req = new GraphQLRequest() { OperationName = "createUser", Variables = new { user = new DarlUserInput { InvoiceEmail = trialEmail, userId = userId, InvoiceName = "Dr Andy's IP Ltd" } }, Query = @"mutation createUser($user: DarlUserInput!){createUser(user: $user){ userId invoiceEmail invoiceName  }}" };
            var resp = await client.PostAsync(req);
            Assert.IsTrue(resp.Errors == null || resp.Errors.Length == 0);
            req = new GraphQLRequest() { OperationName = "getUserById", Variables = new { userId = userId }, Query = @"query getUserById($userId: String!){  userById(userId: $userId){userId, invoiceEmail, invoiceName, accountState}}" };
            resp = await client.PostAsync(req);
            Assert.IsTrue(resp.Errors == null || resp.Errors.Length == 0);
            var user = resp.GetDataFieldAs<DarlUser>("userById");
            Assert.AreEqual(trialEmail, user.InvoiceEmail);
            req = new GraphQLRequest() { OperationName = "updateUser", Variables = new { userId = userId, user = new DarlUserUpdate { accountState = AccountState.admin } }, Query = @"mutation updateUser($userId: String!, $user: DarlUserUpdate!){updateUser(userId: $userId, user: $user){ accountState  }}" };
            resp = await client.PostAsync(req);
            Assert.IsTrue(resp.Errors == null || resp.Errors.Length == 0);
            req = new GraphQLRequest() { OperationName = "getUserById", Variables = new { userId = userId }, Query = @"query getUserById($userId: String!){  userById(userId: $userId){userId, invoiceEmail, invoiceName, accountState}}" };
            resp = await client.PostAsync(req);
            Assert.IsTrue(resp.Errors == null || resp.Errors.Length == 0);
            user = resp.GetDataFieldAs<DarlUser>("userById");
            Assert.AreEqual(AccountState.admin, user.accountState);
            req = new GraphQLRequest() { OperationName = "deleteUser", Variables = new { userId = userId }, Query = @"mutation deleteUser($userId: String!){deleteUser(userId: $userId) { userId }}" };
            resp = await client.PostAsync(req);
            Assert.IsTrue(resp.Errors == null || resp.Errors.Length == 0);
            req = new GraphQLRequest() { OperationName = "getUserById", Variables = new { userId = userId }, Query = @"query getUserById($userId: String!){  userById(userId: $userId){userId, invoiceEmail, invoiceName}}" };
            resp = await client.PostAsync(req);
            Assert.IsTrue(resp.Errors == null || resp.Errors.Length == 0);
            user = resp.GetDataFieldAs<DarlUser>("userById");
            Assert.IsNull(user);

        }

        [TestMethod]
        [Ignore]
        public async Task TestAddAPIKey()
        {
            var req = new GraphQLRequest() { Query = @"{ users {invoiceEmail userId } }" };
            var resp = await client.PostAsync(req);
            var users = resp.GetDataFieldAs<List<DarlUser>>("users");
            foreach(var u in users)
            {
                var apiKey = Guid.NewGuid().ToString();
                req = new GraphQLRequest() { OperationName = "updateUser", Variables = new { userId = u.userId, user = new DarlUserUpdate { apiKey = apiKey } }, Query = @"mutation updateUser($userId: String!, $user: DarlUserUpdate!){  updateUser(userId: $userId, user: $user)  { aPIKey  }}" };
                resp = await client.PostAsync(req);
            }
        }

        [TestMethod]
        public async Task TestFactoryReset()
        {
            var req = new GraphQLRequest() { Query = @"mutation{ factoryReset() }" };
            var resp = await client.PostAsync(req);
        }
    }
}
