using Darl.GraphQL.Models.Models;
using GraphQL.Client;
using GraphQL.Common.Request;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Darl.GraphQL.Test
{
    [TestClass]
    public class CollateralTest
    {
        GraphQLClient client = null;


        [TestInitialize()]
        public void Initialize()
        {
            client = new GraphQLClient("https://darl.dev/graphql/");
            var authcode = "e438440e-9d90-46e8-87ed-080e19c43aed";
            client.DefaultRequestHeaders.Add("Authorization", $"Basic {authcode}");
            client.Options.JsonSerializerSettings.Converters.Add(new StringEnumConverter());
        }


        [TestCleanup()]
        public void Cleanup()
        {

        }

        [TestMethod]
        public async Task TestDeleteCollateral()
        {
            string testName = "rubbishTestValue";
            var req = new GraphQLRequest() { Variables = new { name = testName, value = "testvalue" }, Query = @"mutation uc($name: String!, $value: String!){ updateCollateral(name: $name, value: $value){ name } }" };
            var resp = await client.PostAsync(req);
            var coll = resp.GetDataFieldAs<Collateral>("updateCollateral");
            Assert.AreEqual(coll.Name, testName);
            req = new GraphQLRequest() { Variables = new { name = testName}, Query = @"query gc($name: String!){ getCollateral(name: $name)}" };
            resp = await client.PostAsync(req);
            var text = resp.GetDataFieldAs<string>("getCollateral");
            Assert.AreEqual(text, "testvalue");
            req = new GraphQLRequest() { Variables = new { name = testName }, Query = @"mutation dc($name: String!){deleteCollateral(name: $name){name}}" };
            resp = await client.PostAsync(req);
            req = new GraphQLRequest() { Variables = new { name = testName }, Query = @"query gc($name: String!){ getCollateral(name: $name)}" };
            resp = await client.PostAsync(req);
            text = resp.GetDataFieldAs<string>("getCollateral");
            Assert.AreEqual("",text);
        }

        [Ignore]
        [TestMethod]
        public async Task DeleteBulk()
        {
            string collateralsString = "query {collateral{name,value}}";
            string deletecollateralString = "mutation dc($name: String!){deleteCollateral(name: $name){name}}";
            string botModelName = "far_left.model";

            var req = new GraphQLRequest()
            {
                Query = collateralsString
            };
            var resp = await client.PostAsync(req);
            var collaterals = resp.GetDataFieldAs<List<Collateral>>("collateral");
            int collateralCount = collaterals.Count;
            foreach (var c in collaterals)
            {
                if (c.Name.StartsWith(botModelName))
                {
                    req = new GraphQLRequest()
                    {
                        Variables = new { name = c.Name },
                        Query = deletecollateralString
                    };
                    resp = await client.PostAsync(req);
                }
            }
            req = new GraphQLRequest()
            {
                Query = collateralsString
            };
            resp = await client.PostAsync(req);
            collaterals = resp.GetDataFieldAs<List<Collateral>>("collateral");
            
        }



    }
}
