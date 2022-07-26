using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class AccountStateEnum : EnumerationGraphType
    {

        public AccountStateEnum()
        {
            Name = "AccountState";
            Add("trial", 0, "In a trial period");
            Add("trial_expired", 1, "Trial is over but not signed up");
            Add("paying", 2, "paying for usage");
            Add("delinquent", 3, "late in paying");
            Add("suspended", 4, "suspended for non-payment");
            Add("closed", 5, "usage is terminated");
            Add("admin", 6, "An administrator");
        }
    }
}
