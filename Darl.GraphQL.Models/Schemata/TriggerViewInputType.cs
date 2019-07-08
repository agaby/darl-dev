using Darl.GraphQL.Models.Models;
using DarlCommon;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class TriggerViewInputType : InputObjectGraphType<TriggerViewInput>
    {
        public TriggerViewInputType()
        {
            Name = "triggerInput";
            Description = "A set of conditions and parameters for trigger functionality";
            Field(c => c.addressText, true).Description("Either the name of textual i/o or the fixed email address.");
            Field<SourceTypeEnum>("addressSource", "Is the email address rule-set generated or fixed?");
            Field(c => c.attachmentName, true).Description("The name of the attachment in the email");
            Field(c => c.attachmentUri, true).Description("The source of the attachment in collateral storage");
            Field<SourceTypeEnum>("bodySource", "Is the message body source rule-set generated or fixed?");
            Field(c => c.bodyText, true).Description("Either the name of textual io or the fixed message body");
            Field(c => c.emailFrom, true).Description("The from address for the email");
            Field(c => c.postData, true).Description("The i/o and category that corresponds to a decision to post, or \"true\",\"false\" if fixed.");
            Field<SourceTypeEnum>("postDataSource", "Is the decision to send a POST message rule-set generated or fixed?");
            Field(c => c.postDataUri, true).Description("The uri to send the POST data to");
            Field<PostTypeEnum>("postType", "Format sent data as Json DarlVar list or as form name value pairs.");
            Field(c => c.queueData, true).Description("The i/o and category that corresponds to a decision to queue, or \"true\",\"false\" if fixed.");
            Field<SourceTypeEnum>("queueDataSource", "Is the decision to send a the data to a queue rule-set generated or fixed?");
            Field(c => c.queueName, true).Description("Existing within the Azure storage group accessed by your stored connection string.");
            Field(c => c.sendAttachment).Description("The i/o and category that corresponds to a decision to add the attachment, or \"true\",\"false\" if fixed.");
            Field<SourceTypeEnum>("sendAttachmentSource", "Is the presence of an attachment rule-set generated or fixed?");
            Field(c => c.sendBug, true).Description("The i/o and category that corresponds to a decision to send the bug request, or \"true\",\"false\" if fixed.");
            Field<SourceTypeEnum>("sendBugSource", "Is the sending of the bug request determined by the rule set or fixed?");
            Field(c => c.sendEmail, true).Description("The i/o and category that corresponds to a decision to send the email, or \"true\",\"false\" if fixed.");
            Field<SourceTypeEnum>("sendEmailSource", "Is the sending of the email determined by the rule set or fixed?");
            Field<SourceTypeEnum>("subjectSource", "Is the message subject source rule-set generated or fixed?");
            Field(c => c.subjectText, true).Description("Either the name of textual i/o or the fixed message subject.");
        }
    }
}
