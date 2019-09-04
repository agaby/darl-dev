using Darl.GraphQL.Models.Models;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class UserUsageType : ObjectGraphType<UserUsage>
    {
        public UserUsageType()
        {
            Name = "UserUsage";
            Description = "A day of user usage and the count of interactions.";
            Field(c => c.Date);
            Field(c => c.Count);
        }
    }
}
