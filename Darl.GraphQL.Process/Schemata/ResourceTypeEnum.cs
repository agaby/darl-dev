using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public enum ResourceType { ruleset, mlmodel, botmodel, document, collateral, simulation }
    public class ResourceTypeEnum : EnumerationGraphType
    {
        public ResourceTypeEnum()
        {
            Name = "resourceTypes";
            AddValue("ruleset", "A ruleset model", 0);
            AddValue("mlmodel", "A machine learning model", 1);
            AddValue("botmodel", "A chatbot model", 2);
            AddValue("document", "A document", 3);
            AddValue("collateral", "A piece of collateral", 4);
            AddValue("simulation", "A simulation model", 5);
        }
    }
}
