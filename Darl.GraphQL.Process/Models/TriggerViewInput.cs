/// <summary>
/// TriggerViewInput.cs - Core module for the Darl.dev project.
/// </summary>

﻿using DarlCommon;
using System.ComponentModel.DataAnnotations;

namespace Darl.GraphQL.Models.Models
{
    /// <summary>
    /// TriggerView with all elements nullable for selective updating
    /// </summary>
    public class TriggerViewInput
    {
        [Display(Name = "Queue data source", Description = "Is the decision to send a the data to a queue rule-set generated or fixed?")]
        public SourceType? queueDataSource { get; set; }
        [Display(Name = "The post data type", Description = "Format sent data as json DarlVar list or as form name value pairs.")]
        public PostType? postType { get; set; }
        [Display(Name = "The post data uri", Description = "the uri to send the POST data to")]
        public string postDataUri { get; set; }
        [Display(Name = "Post data decision", Description = "The io and category that corresponds to a decision to post, or \"true\",\"false\" if fixed.")]
        public string postData { get; set; }
        [Display(Name = "Post data source", Description = "Is the decision to send a POST message rule-set generated or fixed?")]
        public SourceType? postDataSource { get; set; }
        [Display(Name = "The attachment uri", Description = "The source of the attachment in blob storage")]
        public string attachmentUri { get; set; }
        [Display(Name = "The attachment name", Description = "The name of the attachment in the email")]
        public string attachmentName { get; set; }
        [Display(Name = "Send attachment decision", Description = "The io and category that corresponds to a decision to add the attachment, or \"true\",\"false\" if fixed.")]
        public string sendAttachment { get; set; }
        [Display(Name = "send attachment source", Description = "Is the presence of an attachment rule-set generated or fixed?")]
        public SourceType? sendAttachmentSource { get; set; }
        [Display(Name = "Queue decision", Description = "The io and category that corresponds to a decision to post, or \"true\",\"false\" if fixed.")]
        public string queueData { get; set; }
        [Display(Name = "Email from", Description = "The from address for the email")]
        public string emailFrom { get; set; }
        [Display(Name = "Body source", Description = "Is the message body source rule-set generated or fixed?")]
        public SourceType? bodySource { get; set; }
        [Display(Name = "Subject text", Description = "Either the name of textual io or the fixed message subject")]
        public string subjectText { get; set; }
        [Display(Name = "Subject source", Description = "Is the message subject source rule-set generated or fixed?")]
        public SourceType? subjectSource { get; set; }
        [Display(Name = "Address text", Description = "Either the name of textual io or the fixed email address")]
        public string addressText { get; set; }
        [Display(Name = "Address source", Description = "Is the email address rule-set generated or fixed?")]
        public SourceType? addressSource { get; set; }
        [Display(Name = "Send Bug decision", Description = "The io and category that corresponds to a decision to send the bug request, or \"true\",\"false\" if fixed.")]
        public string sendBug { get; set; }
        [Display(Name = "Send Bug source", Description = "Is the sending of the bug request determined by the rule set or fixed?")]
        public SourceType? sendBugSource { get; set; }
        [Display(Name = "Send Email decision", Description = "The io and category that corresponds to a decision to send the email, or \"true\",\"false\" if fixed.")]
        public string sendEmail { get; set; }
        [Display(Name = "Send Email source", Description = "Is the sending of the email determined by the rule set or fixed?")]
        public SourceType? sendEmailSource { get; set; }
        [Display(Name = "Body text", Description = "Either the name of textual io or the fixed message body")]
        public string bodyText { get; set; }
        [Display(Name = "Queue name", Description = "Existing within the Azure storage group accessed by your stored connection string.")]
        public string queueName { get; set; }

        public string graphqlData { get; set; }
        public SourceType? graphqlDataSource { get; set; }

    }
}
