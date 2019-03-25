using Darl.GraphQL.Models.Models;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class MailContentType : ObjectGraphType<Collateral>
    {
        public MailContentType()
        {
            Name = "MailContent";
            Description = "Text used for emailing";
            Field(c => c.LastModified);
            Field(c => c.Name);
            Field(c => c.Size);
            Field(c => c.Content);
        }
    }
}
