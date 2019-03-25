using Darl.GraphQL.Models.Models;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class DocumentType : ObjectGraphType<Document>
    {
        public DocumentType()
        {
            Name = "Document";
            Description = "A word document that can be used for reporting";
            Field(c => c.LastModified);
            Field(c => c.Name);
            Field(c => c.Size);
        }
    }
}
