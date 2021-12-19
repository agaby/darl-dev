using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class SubscriptionTypeEnum : EnumerationGraphType
    {
        public SubscriptionTypeEnum()
        {
            Name = "SubscriptionType";
            AddValue("individual", "A single user", 0);
            AddValue("corporate", "A corporate user", 1);
            AddValue("embedded", "A license to embed DARL nugets or docker instances", 2);
            AddValue("inhouse", "A license for DARL associates", 3);
        }
    }
}
