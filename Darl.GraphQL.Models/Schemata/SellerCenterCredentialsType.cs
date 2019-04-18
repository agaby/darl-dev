using Darl.GraphQL.Models.Models;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class SellerCenterCredentialsType : ObjectGraphType<SellerCenterCredentials>
    {
        public SellerCenterCredentialsType()
        {
            Name = "SellerCenterCredentials";
            Description = "Credentials to access the Microsoft Seller Centre.";
            Field(c => c.LiveMode);
            Field(c => c.MerchantId);
            Field(c => c.StripeApiKey);
        }

    }
}
