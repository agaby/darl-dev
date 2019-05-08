using Darl.GraphQL.Models.Models;
using GraphQL.Authorization.AspNetCore;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class ContactType : ObjectGraphType<Contact>
    {
        public ContactType()
        {
            Name = "Contact";
            this.AuthorizeWith("AdminPolicy");

            Description = "A contact that has requested to be informed about DARL.ai";
            Field(c => c.Company,true);
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
            Field<StringGraphType>("Id", resolve: c => c.Source.Id);

        }
    }
}
