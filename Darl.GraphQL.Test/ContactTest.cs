using Darl.Connectivity.Models;
using Darl.GraphQL.Models.Models;
using GraphQL.Client;
using GraphQL.Common.Request;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Darl.GraphQL.Test
{
    [TestClass]
    public class ContactTest
    {
        GraphQLClient client = null;


        [TestInitialize()]
        public void Initialize()
        {
            client = new GraphQLClient("https://darl.dev/graphql/");
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
            req = new GraphQLRequest() { OperationName = "updateContact", Variables = new { contact = new ContactUpdate {  FirstName = "Andrew"} }, Query = @"mutation updateContact($contact ContactUpdate!){updateContact(contact: $contact){ id email lastName  }}" };
            resp = await client.PostAsync(req);
            Assert.IsTrue(resp.Errors == null || resp.Errors.Length == 0);
            req = new GraphQLRequest() { OperationName = "getContactByEmail", Variables = new { email = "andy@darl.ai" }, Query = @"query getContactByEmail($email: String!){  contactByEmail(email: $email){id email lastName firstName}}" };
            resp = await client.PostAsync(req);
            Assert.IsTrue(resp.Errors == null || resp.Errors.Length == 0);
            contact = resp.GetDataFieldAs<Models.Models.Contact>("contactByEmail");
            Assert.AreEqual("Andrew", contact.FirstName);
            req = new GraphQLRequest() { OperationName = "deleteContact", Variables = new { id = contact.Id }, Query = @"mutation deleteContact($id: String!){deleteContact(id: $id) {id }}" };
            resp = await client.PostAsync(req);
            Assert.IsTrue(resp.Errors == null || resp.Errors.Length == 0);
            req = new GraphQLRequest() { OperationName = "getContactByEmail", Variables = new { email = "andy@darl.ai" }, Query = @"query getContactByEmail($email: String!){  contactByEmail(email: $email){id email lastName}}" };
            resp = await client.PostAsync(req);
            Assert.IsTrue(resp.Errors == null || resp.Errors.Length == 0);
            contact = resp.GetDataFieldAs<Models.Models.Contact>("contactByEmail");
            Assert.IsNull(contact);

        }
    }
}
