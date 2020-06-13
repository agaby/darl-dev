using Darl.Connectivity.Models;
using Darl.GraphQL.Models.Models;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Darl.GraphQL.Test
{
/*    [TestClass]
    public class ContactTest
    {
        GraphQLHttpClient client = null;


        [TestInitialize()]
        public void Initialize()
        {
            client = new GraphQLHttpClient("https://darl.dev/graphql/", new NewtonsoftJsonSerializer());
            var authcode = "d70f1008-5758-41b5-9c44-bc90535aeabc";
            client.DefaultRequestHeaders.Add("Authorization", $"Basic {authcode}");
            client.Options.JsonSerializerSettings.Converters.Add(new StringEnumConverter());
        }


        [TestCleanup()]
        public void Cleanup()
        {

        }

        [TestMethod]
        public async Task TestGetContact()
        {
            var req = new GraphQLRequest() { OperationName = "getContactByEmail", Variables = new { email = "ebc@verifile.co.uk" }, Query = @"query getContactByEmail($email: String!){  contactByEmail(email: $email){id email lastName}}" };
            var resp = await client.PostAsync(req);
            Assert.IsTrue(resp.Errors == null || resp.Errors.Length == 0);
            var contact = resp.GetDataFieldAs<Models.Models.Contact>("contactByEmail");
            Assert.AreEqual("ebc@verifile.co.uk", contact.Email);
        }

        [TestMethod]
        public async Task TestSetContact()
        {
            var req = new GraphQLRequest() { OperationName = "createContact", Variables = new { contact = new ContactInput {  Company = "Dr Andy's IP LLC", Email = "andy@darl.ai", FirstName = "Andy", LastName = "Edmonds" } }, Query = @"mutation createContact($contact: ContactInput!){createContact(contact: $contact){ id email lastName  }}" };
            var resp = await client.PostAsync(req);
            Assert.IsTrue(resp.Errors == null || resp.Errors.Length == 0);
            req = new GraphQLRequest() { OperationName = "getContactByEmail", Variables = new { email = "andy@darl.ai" }, Query = @"query getContactByEmail($email: String!){  contactByEmail(email: $email){id email lastName}}" };
            resp = await client.PostAsync(req);
            Assert.IsTrue(resp.Errors == null || resp.Errors.Length == 0);
            var contact = resp.GetDataFieldAs<Models.Models.Contact>("contactByEmail");
            Assert.AreEqual("andy@darl.ai", contact.Email);
            req = new GraphQLRequest() { OperationName = "updateContact", Variables = new { contact = new ContactUpdate {  FirstName = "Andrew", Email = "andy@darl.ai"} }, Query = @"mutation updateContact($contact: ContactUpdate!){updateContact(contact: $contact){ id email lastName  }}" };
            resp = await client.PostAsync(req);
            Assert.IsTrue(resp.Errors == null || resp.Errors.Length == 0);
            req = new GraphQLRequest() { OperationName = "getContactByEmail", Variables = new { email = "andy@darl.ai" }, Query = @"query getContactByEmail($email: String!){  contactByEmail(email: $email){id email lastName firstName}}" };
            resp = await client.PostAsync(req);
            Assert.IsTrue(resp.Errors == null || resp.Errors.Length == 0);
            contact = resp.GetDataFieldAs<Models.Models.Contact>("contactByEmail");
            Assert.AreEqual("Andrew", contact.FirstName);
            req = new GraphQLRequest() { OperationName = "deleteContact", Variables = new { email = contact.Email }, Query = @"mutation deleteContact($email: String!){deleteContact(email: $email) {id }}" };
            resp = await client.PostAsync(req);
            Assert.IsTrue(resp.Errors == null || resp.Errors.Length == 0);
            req = new GraphQLRequest() { OperationName = "getContactByEmail", Variables = new { email = "andy@darl.ai" }, Query = @"query getContactByEmail($email: String!){  contactByEmail(email: $email){id email lastName}}" };
            resp = await client.PostAsync(req);
            Assert.IsTrue(resp.Errors == null || resp.Errors.Length == 0);
            contact = resp.GetDataFieldAs<Models.Models.Contact>("contactByEmail");
            Assert.IsNull(contact);

        }

        [TestMethod]
        public async Task TestreportPurchase()
        {
            var email = "ed.guiness@verifile.co.uk";
            var name = "ed Guiness";
            var sessionId = "jhkjhkjlkjlkj";
            var date = DateTime.UtcNow;
            var req = new GraphQLRequest() { Variables = new { email = email, name = name, date = date, sessionId = sessionId }, Query = @"mutation reportPurchase($email: String!, $name: String!, $sessionId: String!, $date: DateTime!){ reportPurchase(email: $email, name: $name, sessionId: $sessionId, date: $date){sessionId}}" };
            var resp = await client.PostAsync(req);

        }
    }*/
}
