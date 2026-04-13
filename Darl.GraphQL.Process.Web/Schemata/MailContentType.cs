/// <summary>
/// </summary>

﻿using Darl.GraphQL.Models.Models;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class MailContentType : ObjectGraphType<Collateral>
    {
        public MailContentType()
        {
            Name = "MailContent";
            Description = "Text used for emailing";
            Field(c => c.Name);
            Field(c => c.Value);
        }
    }
}
