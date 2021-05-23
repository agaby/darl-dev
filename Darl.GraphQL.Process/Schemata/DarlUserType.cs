using Darl.GraphQL.Models.Middleware;
using Darl.GraphQL.Models.Models;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class DarlUserType : ObjectGraphType<DarlUser>
    {

        public DarlUserType()
        {
            Name = "darlUser";
            Description = "A user of the system - admin only";
            this.AuthorizeWith("AdminPolicy");
            Field<AccountStateEnum>("accountState", resolve: c => c.Source.accountState);
            Field(c => c.Created);
            Field(c => c.current_period_end);
            Field(c => c.InvoiceEmail);
            Field(c => c.InvoiceName, true);
            Field(c => c.InvoiceOrganization,true);
            Field(c => c.Issuer, true);
            Field(c => c.PaidUsageStarted);
            Field(c => c.StripeCustomerId,true);
            Field(c => c.productId,true);
            Field(c => c.userId);
            Field(c => c.APIKey);
            Field<SubscriptionTypeEnum>("subscriptionType", resolve: c => c.Source.subscriptionType);
        }
    }
}
