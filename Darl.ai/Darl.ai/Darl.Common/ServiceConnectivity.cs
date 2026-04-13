/// </summary>

﻿using System.ComponentModel.DataAnnotations;

namespace DarlCommon
{
    public class ServiceConnectivity
    {
        [Display(Name = "Seller center connection", Description = "Connect your bot to the Microsoft seller center and Stripe to take payments.")]
        public SellerCenterCredentials sellercred { get; set; }

        [Display(Name = "SendGrid connection", Description = "Connect your bot to SendGrid to send emails.")]
        public SendGridCredentials sendgridcred { get; set; }

        [Display(Name = "Twilio connection", Description = "Connect your bot to Twilio to send texts.")]
        public TwilioCredentials twiliocred { get; set; }

        [Display(Name = "Azure storage connection", Description = "Connect your bot to Azure storage to Queue events.")]
        public AzureCredentials azurecred { get; set; }

        [Display(Name = "ZenDesk connection", Description = "Connect your bot to Zendesk to report bugs.")]
        public ZendeskCredentials zendeskcred { get; set; }

        [Display(Name = "GraphQL connection", Description = "Connect your bot to a GraphQL endpoint.")]
        public GraphQLCredentials graphqlcred { get; set; }

    }
}
