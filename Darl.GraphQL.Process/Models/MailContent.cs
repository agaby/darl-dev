using System;

namespace Darl.GraphQL.Models.Models
{
    public class MailContent
    {
        public MailContent(DateTime lastModified, string name, int size, string content)
        {
            LastModified = lastModified;
            Name = name;
            Size = size;
            Content = content;
        }

        public DateTime LastModified { get; }
        public string Name { get; }
        public int Size { get; }
        public string Content { get; }
    }
}
