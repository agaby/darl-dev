using Darl.GraphQL.Models.Models;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class DarlUserInputType : InputObjectGraphType<DarlUserInput>
    {
        public DarlUserInputType()
        {
            Name = "DarlUserInput";
            Field<NonNullGraphType<StringGraphType>>("userId");
            Field<StringGraphType>("issuer");
            Field<NonNullGraphType<DateTimeGraphType>>("created");
            Field<DateTimeGraphType>("paidUsageStarted");
            Field<StringGraphType>("invoiceName");
            Field<StringGraphType>("invoiceOrganization");
            Field<NonNullGraphType<StringGraphType>>("invoiceEmail");
            Field<DateTimeGraphType>("current_period_end");
            Field<StringGraphType>("stripeCustomerId");
            Field<StringGraphType>("usageStripeSubscriptionItem");
        }
    }
}
