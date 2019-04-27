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
    public class UserTest
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
        public async Task TestGetUser()
        {
            var req = new GraphQLRequest() { OperationName = "getUserByEmail", Variables = new { email = "andy@scientio.com" }, Query = @"query getUserByEmail($email: String!){  usersByEmail(email: $email){id InvoiceEmail}}" };
            var resp = await client.PostAsync(req);
            Assert.IsTrue(resp.Errors == null || resp.Errors.Length == 0);
            var users = resp.GetDataFieldAs<List<Models.Models.Contact>>("usersByEmail");

        }
    }
}
