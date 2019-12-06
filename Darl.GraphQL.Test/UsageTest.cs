using Darl.GraphQL.Models.Models;
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
    public class UsageTest
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
        public async Task TestRulesetUsage()
        {
            //create a dummy ruleset
            var req = new GraphQLRequest() { Query = "mutation { createEmptyRuleset(name: \"stupidname.ruleset\"){name  }}" };
            var resp = await client.PostAsync(req);
            //add a usage or two
            req = new GraphQLRequest() { Query = "mutation { createRulesetUsage(userId: \"5ee43551-c05c-4cff-8582-c08f23f84c1\",model: \"stupidname.ruleset\", date: \"06/11/1955\", count: 55 ){date count}}" };
            resp = await client.PostAsync(req);
            //check it's there
            req = new GraphQLRequest() { Query = "query { rulesetByName(name: \"stupidname.ruleset\"){name usageHistory{date count}  }}" };
            resp = await client.PostAsync(req);
            var rs = resp.GetDataFieldAs<RuleSet>("rulesetByName");
            Assert.AreEqual(1, rs.UsageHistory.Count);
            //check duplicate isn't added
            req = new GraphQLRequest() { Query = "mutation { createRulesetUsage(userId: \"5ee43551-c05c-4cff-8582-c08f23f84c1\",model: \"stupidname.ruleset\", date: \"06/11/1955\", count: 55 ){date count}}" };
            resp = await client.PostAsync(req);
            req = new GraphQLRequest() { Query = "query { rulesetByName(name: \"stupidname.ruleset\"){name usageHistory{date count}  }}" };
            resp = await client.PostAsync(req);
            rs = resp.GetDataFieldAs<RuleSet>("rulesetByName");
            Assert.AreEqual(1, rs.UsageHistory.Count);
            //delete ruleset
            req = new GraphQLRequest() { Query = "mutation { deleteRuleSet(name: \"stupidname.ruleset\"){name  }}" };
            resp = await client.PostAsync(req);
        }
    }
}
