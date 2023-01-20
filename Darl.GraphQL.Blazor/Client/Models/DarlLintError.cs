namespace Darl.GraphQL.Blazor.Client.Models
{
    public class DarlLintError
    {
        public int column_no_start { get; set; }
        public int column_no_stop { get; set; }
        public int line_no { get; set; }
        public string message { get; set; } = string.Empty;
        public string severity { get; set; } = string.Empty; 
    }
}
