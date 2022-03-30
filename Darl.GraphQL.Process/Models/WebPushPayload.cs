namespace Darl.GraphQL.Models.Models
{
    public class WebPushPayload
    {
        public string title { get; set; } = string.Empty;

        public WebPushOptions? options { get; set; }
    }
}
