using Darl.Connectivity.Models;
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
            Field(c => c.LiveMode);
            Field(c => c.MerchantId);
            Field(c => c.StripeApiKey);
        }

    }
}
