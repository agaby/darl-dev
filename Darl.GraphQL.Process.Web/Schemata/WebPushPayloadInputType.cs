using Darl.GraphQL.Models.Models;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Schemata
{
    public class WebPushPayloadInputType : InputObjectGraphType<WebPushPayload>
    {
        public WebPushPayloadInputType()
        {
            Name = "webPushPayloadInput";
            Description = "A web push notification.";
            Field<ListGraphType<WebPushOptionsInputType>>("options", "The set of options possible for this notification", resolve: context => context.Source.options);
            Field(c => c.title, true).Description("The text headline.");

        }
    }
}
