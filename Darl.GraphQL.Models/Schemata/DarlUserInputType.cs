using Darl.GraphQL.Models.Models;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class DarlUserInputType : InputObjectGraphType<DarlUser>
    {
        public DarlUserInputType()
        {
            Name = "darlUserInput";
            Field<NonNullGraphType<StringGraphType>>("userId");
            Field<StringGraphType>("issuer");
            Field<NonNullGraphType<DateTimeGraphType>>("created");
            Field<NonNullGraphType<DateTimeGraphType>>("paidUsageStarted");
            Field<StringGraphType>("invoiceName");
            Field<StringGraphType>("invoiceOrganization");
            Field<NonNullGraphType<StringGraphType>>("invoiceEmail");
            Field<AccountStateEnum>("accountState");
            Field<NonNullGraphType<DateTimeGraphType>>("current_period_end");
            Field<StringGraphType>("stripeCustomerId");
            Field<StringGraphType>("usageStripeSubscriptionItem");
            Field<ListGraphType<UserUsageType>>("usageHistory");
        }
    }
}
