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
            var authcode = "86f98f0e-0b42-409b-a304-44de4bd99113";
            client.DefaultRequestHeaders.Add("Authorization", $"Basic {authcode}");
            client.Options.JsonSerializerSettings.Converters.Add(new StringEnumConverter());
        }

        [TestMethod]
        public async Task TestInteract()
        {
            var req = new GraphQLRequest()
            {
                Variables = new { model = "thousandquestions.model", convId = "aaabbb", data = new DarlVar { name = "", Value = "hi", dataType = DarlVar.DataType.textual } },
                Query = @"query Interact($model: String!, $convId: String!, $data: darlVarUpdate!){ interact(botModelName: $model, conversationId: $convId, conversationData: $data){ response { value dataType } }}",
                OperationName = "Interact"
            };
            var resp = await client.PostAsync(req);
            var responses = resp.GetDataFieldAs<List<InteractTestResponse>>("interact");

        }
    }

}
