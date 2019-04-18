using Darl.GraphQL.Models.Models;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class ServiceConnectivityType : ObjectGraphType<ServiceConnectivity>
    {
        public ServiceConnectivityType()
        {
            Name = "ServiceConnectivity";
            Description = "Credentials to access external services.";
            Field<AzureCredentialsType>("azureCred", resolve: context => context.Source.azurecred);
            Field<SellerCenterCredentialsType>("sellerCred", resolve: context => context.Source.sellercred);
            Field<TwilioCredentialsType>("twilioCred", resolve: context => context.Source.twiliocred);
            Field<SendGridCredentialsType>("sendgridCred", resolve: context => context.Source.sendgridcred);
            Field<ZendeskCredentialsType>("zendeskCred", resolve: context => context.Source.zendeskcred);
        }
    }
}
