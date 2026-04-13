/// <summary>
/// </summary>

﻿using Darl.GraphQL.Models.Models;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class ContactInputType : InputObjectGraphType<Contact>
    {
        public ContactInputType()
        {
            Name = "ContactInput";
            Description = "A contact that has requested to be informed about DARL.ai";
            Field(c => c.Company, true);
            Field(c => c.Country, true);
            Field(c => c.Email);
            Field(c => c.FirstName, true);
            Field(c => c.IntroSent, true).DefaultValue(false);
            Field(c => c.IPAddress, true);
            Field(c => c.LastName, true);
            Field(c => c.Notes, true);
            Field(c => c.Phone, true);
            Field(c => c.Sector, true);
            Field(c => c.Source, true);
            Field(c => c.latitude, true);
            Field(c => c.longitude, true);
        }

    }
}
