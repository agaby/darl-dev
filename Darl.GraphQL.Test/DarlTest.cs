using DarlCommon;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Darl.GraphQL.Test
{
    [TestClass]
    public class DarlTest
    {
        GraphQLHttpClient client = null;


        [TestInitialize()]
        public void Initialize()
        {
            client = new GraphQLHttpClient("https://darl.dev/graphql/", new NewtonsoftJsonSerializer());

        }


        [TestCleanup()]
        public void Cleanup()
        {

        }

        [TestMethod]
        public async Task TestProcessRuleSet()
        {
            var req = new GraphQLHttpRequest() { OperationName = "getExampleInputs", Variables = new { name = "UK Tax and NI.rule" }, Query = @"query getExampleInputs($name: String!){ getExampleInputs(ruleSetName: $name ) { name value dataType}}" };
            var resp = await client.SendQueryAsync<List<DarlVar>>(req);
//            var inputs = resp.GetDataFieldAs<List<DarlVar>>("getExampleInputs");
//            req = new GraphQLRequest() { OperationName = "inferFromRuleSet", Variables = new { name = "UK Tax and NI.rule", data = inputs }, Query = @"query inferFromRuleSet($name: String!, $data: [darlVarInput]!){ inferFromRuleSet(ruleSetName: $name, inputs: $data ) { name value dataType }}" };
 //           resp = await client.PostAsync(req);
//            var outputs = resp.GetDataFieldAs<List<DarlVar>>("inferFromRuleSet");

        }
    }
}
