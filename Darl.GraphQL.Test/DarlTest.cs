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
    public class DarlTest
    {
        GraphQLClient client = null;


        [TestInitialize()]
        public void Initialize()
        {
            client = new GraphQLClient("https://darl.dev/graphql/");
            client.Options.JsonSerializerSettings.Converters.Add(new StringEnumConverter());

        }


        [TestCleanup()]
        public void Cleanup()
        {

        }

        [TestMethod]
        public async Task TestProcessRuleSet()
        {
            var req = new GraphQLRequest() { OperationName = "getExampleInputs", Variables = new { name = "UK Tax and NI.rule" }, Query = @"query getExampleInputs($name: String!){ getExampleInputs(ruleSetName: $name ) { name value dataType}}" };
            var resp = await client.PostAsync(req);
            var inputs = resp.GetDataFieldAs<List<DarlVar>>("getExampleInputs");
            req = new GraphQLRequest() { OperationName = "inferFromRuleSet", Variables = new { name = "UK Tax and NI.rule", data = inputs }, Query = @"query inferFromRuleSet($name: String!, $data: [darlVarUpdate]!){ inferFromRuleSet(ruleSetName: $name, inputs: $data ) { name value dataType }}" };
            resp = await client.PostAsync(req);
            var outputs = resp.GetDataFieldAs<List<DarlVar>>("inferFromRuleSet");

        }
    }
}
