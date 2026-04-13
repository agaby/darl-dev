/// <summary>
/// DefaultTest.cs - Core module for the Darl.dev project.
/// </summary>

﻿namespace Darl.GraphQL.Test
{
    /*    [TestClass]
        public class DefaultTest
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
            public async Task TestGetDefault()
            {
                var req = new GraphQLRequest() { OperationName = "getDefaultValue", Variables = new {name = "TwitterHashtags" }, Query = @"query getDefaultValue($name: String!){  defaultValue(name: $name)}" };
                var  resp = await client.PostAsync(req);
                var text = resp.GetDataFieldAs<string>("defaultValue");
                Assert.AreEqual("chatbot,expertsystem,fuzzy,rulesengine", text);
            }

            [TestMethod]
            public async Task TestSetDefault()
            {
                var req = new GraphQLRequest() { OperationName = "createDefault", Variables = new { name = "whifflepoop", value = "poopies" }, Query = @"mutation createDefault($name: String!, $value: String!){createDefault(name: $name, value: $value) {name value }}" };
                var resp = await client.PostAsync(req);
                Assert.IsTrue(resp.Errors == null || resp.Errors.Length == 0);
                req = new GraphQLRequest() { OperationName = "getDefaultValue", Variables = new { name = "whifflepoop" }, Query = @"query getDefaultValue($name: String!){  defaultValue(name: $name)}" };
                resp = await client.PostAsync(req);
                Assert.IsTrue(resp.Errors == null || resp.Errors.Length == 0);
                var text = resp.GetDataFieldAs<string>("defaultValue");
                Assert.AreEqual("poopies", text);
                req = new GraphQLRequest() { OperationName = "updateDefault", Variables = new { name = "whifflepoop", value = "farts" }, Query = @"mutation updateDefault($name: String!, $value: String!){updateDefault(name: $name, value: $value) {name value }}" };
                resp = await client.PostAsync(req);
                Assert.IsTrue(resp.Errors == null || resp.Errors.Length == 0);
                req = new GraphQLRequest() { OperationName = "getDefaultValue", Variables = new { name = "whifflepoop" }, Query = @"query getDefaultValue($name: String!){  defaultValue(name: $name)}" };
                resp = await client.PostAsync(req);
                text = resp.GetDataFieldAs<string>("defaultValue");
                Assert.AreEqual("farts", text);
                Assert.IsTrue(resp.Errors == null || resp.Errors.Length == 0);
                req = new GraphQLRequest() { OperationName = "deleteDefault", Variables = new { name = "whifflepoop"}, Query = @"mutation deleteDefault($name: String!){deleteDefault(name: $name) {name value }}" };
                resp = await client.PostAsync(req);
                Assert.IsTrue(resp.Errors == null || resp.Errors.Length == 0);
                req = new GraphQLRequest() { OperationName = "getDefaultValue", Variables = new { name = "whifflepoop" }, Query = @"query getDefaultValue($name: String!){  defaultValue(name: $name)}" };
                resp = await client.PostAsync(req);
                text = resp.GetDataFieldAs<string>("defaultValue");
                Assert.AreEqual("", text);
                Assert.IsTrue(resp.Errors == null || resp.Errors.Length == 0);
            }

        }*/
}
