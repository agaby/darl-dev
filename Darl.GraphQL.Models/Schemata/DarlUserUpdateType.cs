using Darl.GraphQL.Models.Models;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class DarlUserUpdateType : InputObjectGraphType<DarlUserUpdate>
    {
        public DarlUserUpdateType()
        {
            Name = "DarlUserUpdate";
            Description = "A user who has registered with the DARL system";
            Field<DateTimeGraphType>("PaidUsageStarted");
            Field<StringGraphType>("InvoiceName");
            Field<StringGraphType>("InvoiceOrganization");
            Field<StringGraphType>("InvoiceEmail");
            Field<DateTimeGraphType>("current_period_end");
            Field<StringGraphType>("StripeCustomerId");
            Field<StringGraphType>("UsageStripeSubscriptionItem");
            Field<AccountStateEnum>("accountState");
        }
    }
}
