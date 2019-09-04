using DarlCommon;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class TriggerViewType : ObjectGraphType<TriggerView>
    {

        public TriggerViewType()
        {
            Name = "Trigger";
            Description = "Definitions of the actions triggered by a questionnaire being completed..";
            Field(c => c.addressText,true);
            Field<SourceTypeEnum>("addressSource", resolve: context => context.Source.addressSource);
            Field(c => c.attachmentName, true);
            Field(c => c.attachmentUri, true);
            Field<SourceTypeEnum>("bodySource", resolve: context => context.Source.bodySource);
            Field(c => c.bodyText, true);
            Field(c => c.emailFrom, true);
            Field(c => c.postData, true);
            Field<SourceTypeEnum>("postDataSource", resolve: context => context.Source.postDataSource);
            Field(c => c.postDataUri, true);
            Field<PostTypeEnum>("postType", resolve: context => context.Source.postType);
            Field(c => c.queueData, true);
            Field<SourceTypeEnum>("queueDataSource", resolve: context => context.Source.queueDataSource);
            Field(c => c.queueName, true);
            Field(c => c.sendAttachment, true);
            Field<SourceTypeEnum>("sendAttachmentSource", resolve: context => context.Source.sendAttachmentSource);
            Field(c => c.sendBug, true);
            Field<SourceTypeEnum>("sendBugSource", resolve: context => context.Source.sendBugSource);
            Field(c => c.sendEmail, true);
            Field<SourceTypeEnum>("sendEmailSource", resolve: context => context.Source.sendEmailSource);
            Field<SourceTypeEnum>("subjectSource", resolve: context => context.Source.subjectSource);
            Field(c => c.subjectText, true);
        }
    }
}
 