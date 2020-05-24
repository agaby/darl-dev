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
            client = new GraphQLClient("https://darlgraphql-stagng.azurewebsites.net/graphql/");
            var authcode = "8952d1af-9d34-4866-a4bc-412bf51743d6";
            client.DefaultRequestHeaders.Add("Authorization", $"Basic {authcode}");
            client.Options.JsonSerializerSettings.Converters.Add(new StringEnumConverter());
        }

        [TestMethod]
        public async Task TestInteract()
        {
            var req = new GraphQLRequest()
            {
                Variables = new { model = "thousandquestions.model", convId = "aaabbb", data = new { name = "", Value = "who are tyou", dataType = DarlVar.DataType.textual } },
                Query = @"query Interact($model: String!, $convId: String!, $data: darlVarInput!){ interact(botModelName: $model, conversationId: $convId, conversationData: $data){ response { value dataType } }}",
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
                Query = @"query Interact($model: String!, $convId: String!, $data: darlVarInput!){ interact(botModelName: $model, conversationId: $convId, conversationData: $data){ response { value dataType approximate} }}",
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
                Query = "mutation {createEmptyBotModel(name: \"FarLeftGraph.model\"){name}}" 
            };
            var resp = await client.PostAsync(req);
            //add a trigger and response
            req = new GraphQLRequest()
            {
                Query = "mutation cp($path: String!, $darl: String!){createPhrase(botModelName: \"FarLeftGraph.model\", path: $path, attribute: { darl: $darl }) {definition}}",
                Variables = new { path = "who/is/value:text", darl= "output textual val;\n if anything then val will be Value[\"value:text\"];\n if anything then response will be Graph[\"text\",\"noun:00,2,00\",val];" }
                };
            resp = await client.PostAsync(req);
            //test 
            req = new GraphQLRequest()
            {
                Query = "query interact($model: String!, $convId: String!, $data: darlVarInput!){interact(botModelName: $model, conversationId: $convId, conversationData: $data) {response{value dataType } }}",
                Variables = new { model = "FarLeftGraph.model", convId = "aaabb", data = new { name = "", Value = "who is Jeremy Corbyn", dataType = DarlVar.DataType.textual } }
            };
            resp = await client.PostAsync(req);

            //delete
            
            
            req = new GraphQLRequest()
            {
                Query = "mutation {deleteBotModel(name: \"FarLeftGraph.model\"){name}}"
            };
            resp = await client.PostAsync(req);

        }
    }

}
