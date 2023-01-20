namespace Darl.GraphQL.Blazor.Client.Models
{
    public class LineageRecordResponse
    {
        public enum GraphElementTypes
        {
            NODE,
            CONNECTION,
            ATTRIBUTE
        }

        public List<LineageRecord> getLineagesForWord { get;set; }
        public List<LineageRecord> getLineagesInKG { get;set; }
    }
}
