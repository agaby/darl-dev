using Darl.GraphQL.Models.Models;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class DocumentType : ObjectGraphType<Document>
    {
        public DocumentType()
        {
            Name = "Document";
            Description = "A word document that can be used for reporting";
            Field(c => c.userId).Description("The owner of the document");
            Field(c => c.name).Description("The document name");
            Field(c => c.content).Description("The document content");
        }
    }
}
