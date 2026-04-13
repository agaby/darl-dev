/// <summary>
/// </summary>

﻿using Darl.GraphQL.Models.Models;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class DarlLicenseType : ObjectGraphType<DarlLicense>
    {
        public DarlLicenseType()
        {
            Name = "darlLicense";
            //           this.AuthorizeWith("AdminPolicy");

            Description = "A License issued for DARL products.";
            Field(c => c.licensekey);
            Field(c => c.terminates);
            Field(c => c.created);

        }
    }
}
