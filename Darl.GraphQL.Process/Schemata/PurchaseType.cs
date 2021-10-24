using Darl.GraphQL.Models.Models;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class PurchaseType : ObjectGraphType<Purchase>
    {

        public PurchaseType()
        {
            Name = "purchase";
            Description = "record of a purchase made via stripe";
            Field(c => c.date).Description("date and time of the purchase");
            Field(c => c.sessionId).Description("Id in the stripe system");
        }
    }
}
