using Darl.GraphQL.Models.Models;
using GraphQL.Authorization.AspNetCore;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class DarlUserType : ObjectGraphType<DarlUser>
    {

        public DarlUserType()
        {
            Name = "darlUser";
//           this.AuthorizeWith("AdminPolicy");

            Field<AccountStateEnum>("accountState", resolve: c => c.Source.accountState);
            Field(c => c.Created);
            Field(c => c.current_period_end);
            Field(c => c.InvoiceEmail);
            Field(c => c.InvoiceName);
            Field(c => c.InvoiceOrganization);
            Field(c => c.Issuer);
            Field(c => c.PaidUsageStarted);
            Field(c => c.StripeCustomerId);
            Field(c => c.UsageStripeSubscriptionItem);
            Field(c => c.userId);
            Field(c => c.APIKey);
            Field<ListGraphType<UserUsageType>>("usageHistory", resolve: context => context.Source.UsageHistory);

        }
    }
}
