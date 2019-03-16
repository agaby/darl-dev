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
            Field(c => c.addressText);
            Field(c => c.addressSource);
            Field(c => c.attachmentName);
            Field(c => c.attachmentUri);
            Field<SourceTypeEnum>("bodySource", resolve: context => context.Source.bodySource);
            Field(c => c.bodyText);
            Field(c => c.emailFrom);
            Field(c => c.postData);
            Field<SourceTypeEnum>("postDataSource", resolve: context => context.Source.postDataSource);
            Field(c => c.postDataUri);
            Field<PostTypeEnum>("postType", resolve: context => context.Source.postType);
            Field(c => c.queueData);
            Field<SourceTypeEnum>("queueDataSource", resolve: context => context.Source.queueDataSource);
            Field(c => c.queueName);
            Field(c => c.sendAttachment);
            Field<SourceTypeEnum>("sendAttachmentSource", resolve: context => context.Source.sendAttachmentSource);
            Field(c => c.sendBug);
            Field<SourceTypeEnum>("sendBugSource", resolve: context => context.Source.sendBugSource);
            Field(c => c.sendEmail);
            Field<SourceTypeEnum>("sendEmailSource", resolve: context => context.Source.sendEmailSource);
            Field<SourceTypeEnum>("subjectSource", resolve: context => context.Source.subjectSource);
            Field(c => c.subjectText);
        }
    }
}
 