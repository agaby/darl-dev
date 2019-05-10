using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Darl.GraphQL
{
    public class AppSettings
    {
        public string StorageConnectionString { get; set; }
        public string boaiuserid { get; set; }
        public string modelcontainer { get; set; }
        public string rulecontainer { get; set; }
        public string docscontainer { get; set; }
        public string collateralcontainer { get; set; }
        public string mlmodelcontainer { get; set; }
        public string newscontentcontainer { get; set; }
        public string mailcontentcontainer { get; set; }
        public string MongoConnectionString { get; set; }
        public string MongoDatabase { get; set; }
        public string GremlinHostName { get; set; }
        public string GremlinAuthKey { get; set; }
        public string StripeAPIKey { get; set; }
        public string StripeCorporateLicensePlan { get; set; }
        public string StripeIndividualLicensePlan { get; set; }
        public string StripeCorporateUsagePlan { get; set; }
        public string StripeIndividualUsagePlan { get; set; }
        public string StripeWebHookSecret { get; set; }
        public bool StripeTest { get; set; }
        public int StripeTrialPeriodDays { get; set; }
        public string ProvisionBotModel { get; set; }
        public string ProvisionRulesets { get; set; }
        public string ProvisionMLModels { get; set; }
    }
}
