/// <summary>
/// </summary>

﻿using Darl.GraphQL.Models.Models;
using GraphQL;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class ContactType : ObjectGraphType<Contact>
    {
        public ContactType()
        {
            Name = "Contact";
            this.AuthorizeWith("AdminPolicy");

            Description = "A contact that has requested to be informed about DARL.ai";
            Field(c => c.Company, true);
            Field(c => c.Country, true);
            Field(c => c.Created, true);
            Field(c => c.Email);
            Field(c => c.FirstName, true);
            Field(c => c.IntroSent, true);
            Field(c => c.LastName, true);
            Field(c => c.Notes, true);
            Field(c => c.Phone, true);
            Field(c => c.Sector, true);
            Field(c => c.Source, true);
            Field(c => c.Title, true);
            Field(c => c.latitude, true);
            Field(c => c.longitude, true);
            Field(c => c.IPAddress, true);
            Field<StringGraphType>("Id", resolve: c => c.Source.Id);
            Field<ListGraphType<PurchaseType>>("purchases", resolve: c => c.Source.purchases);
            Field<ListGraphType<DarlLicenseType>>("licenses", resolve: c => c.Source.licenses);
        }
    }
}
