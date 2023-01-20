namespace Darl.GraphQL.Blazor.Client.Models
{
    public class LineageRecord
    {

        public enum LineageTypes
        {
            CONCEPT,
            REFERENCE,
            VALUE,
            LITERAL,
            DEFAULT,
            COMPOSITE
        }
        public string typeWord { get; set; }

        public string lineage { get; set; }

        public LineageTypes type { get; set; }

        public string description { get; set; }
    }
}
