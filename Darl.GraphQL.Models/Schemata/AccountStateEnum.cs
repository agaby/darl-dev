using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class AccountStateEnum : EnumerationGraphType
    {

        public AccountStateEnum()
        {
            Name = "accountState";
            AddValue("trial", "In a trial period", 0);
            AddValue("trial_expired", "Trial is over but not signed up", 1);
            AddValue("paying", "paying for usage", 2);
            AddValue("delinquent", "late in paying", 3);
            AddValue("suspended", "suspended for non-payment", 4);
            AddValue("closed", "usage is terminated", 5);
            AddValue("admin", "An administrator", 6);
        }
    }
}
