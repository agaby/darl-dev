using Darl.Lineage.Bot;
using DarlCommon;
using GraphQL.Client;
using GraphQL.Common.Request;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Darl.GraphQL.Test
{
    [TestClass]
    public class InteractTests
    {
        GraphQLClient client = null;


        [TestInitialize()]
        public void Initialize()
        {
            client = new GraphQLClient("https://darl.dev/graphql/");
            var authcode = "8952d1af-9d34-4866-a4bc-412bf51743d6";
            client.DefaultRequestHeaders.Add("Authorization", $"Basic {authcode}");
            client.Options.JsonSerializerSettings.Converters.Add(new StringEnumConverter());
        }

        [TestMethod]
        public async Task TestInteract()
        {
            var req = new GraphQLRequest()
            {
                Variables = new { model = "thousandquestions.model", convId = "aaabbb", data = new { name = "", Value = "hi", dataType = DarlVar.DataType.textual } },
                Query = @"query Interact($model: String!, $convId: String!, $data: darlVarUpdate!){ interact(botModelName: $model, conversationId: $convId, conversationData: $data){ response { value dataType } }}",
                OperationName = "Interact"
            };
            var resp = await client.PostAsync(req);
            var responses = resp.GetDataFieldAs<List<InteractTestResponse>>("interact");
            Assert.AreEqual(1, responses.Count);

        }

        [TestMethod]
        public async Task TestDefaultHandling()
        {
            var req = new GraphQLRequest()
            {
                Variables = new { model = "thousandquestions.model", convId = "aaabbb", data = new { name = "", Value = "schnerbblewerble", dataType = DarlVar.DataType.textual } },
                Query = @"query Interact($model: String!, $convId: String!, $data: darlVarUpdate!){ interact(botModelName: $model, conversationId: $convId, conversationData: $data){ response { value dataType approximate} }}",
                OperationName = "Interact"
            };
            var resp = await client.PostAsync(req);
            var responses = resp.GetDataFieldAs<List<InteractTestResponse>>("interact");
            Assert.IsTrue(responses[0].response.approximate);

        }

        [TestMethod]
        public async Task TestGraphStore()
        {
            //create new botmodel 
            var req = new GraphQLRequest()
            {

            };
            var resp = await client.PostAsync(req);
                //add A trigger and response
                //test
                //delete

        }
    }

}
