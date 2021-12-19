using Darl.GraphQL.Models.Models;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Schemata
{
    public class WebPushOptionsInputType : InputObjectGraphType<WebPushOptions>
    {
        public WebPushOptionsInputType()
        {
            Name = "webPushOptionsInput";
            Description = "Options for a web push notification.";
            Field<ListGraphType<WebPushActionInputType>>("actions", "The set of actions possible for this notification", resolve: context => context.Source.actions);
            Field(c => c.body, true).Description("The text body.");
            Field(c => c.badge,true).Description("URL to a badge icon.");
            Field(c => c.dir, true).Description("Text direction.");
            Field(c => c.icon, true).Description("URL to icon.");
            Field(c => c.image, true).Description("URL to image");
            Field(c => c.renotify, true).Description("determines if a notification is generated if the same tag is re-used. Default false.");
            Field(c => c.requireInteraction, true).Description("Determines if the notification requires a click before timing out. Default false.");
            Field(c => c.silent, true).Description("Determines if the notification is silent. Default false.");
            Field(c => c.tag, true).Description("Identifier for a notification");
            Field(c => c.timeStamp, true).Description("Timestamp.");
            Field<ListGraphType<IntGraphType>>("vibrate", "Sequence of millisecond vibrate timings, odd active, even inactive", resolve: context => context.Source.vibrate);
        }
    }
}
