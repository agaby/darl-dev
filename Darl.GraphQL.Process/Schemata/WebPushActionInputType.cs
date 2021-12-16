using Darl.GraphQL.Models.Models;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Schemata
{
    public class WebPushActionInputType : InputObjectGraphType<WebPushAction>
    {
        public WebPushActionInputType()
        {
            Name = "webPushActionInput";
            Description = "An action element in a web push notification.";
            Field(c => c.action).Description("A DOMString identifying a user action to be displayed on the notification.");
            Field(c => c.title).Description("A DOMString containing action text to be shown to the user");
            Field(c => c.icon).Description("A USVString containing the URL of an icon to display with the action.");
        }
    }
}
