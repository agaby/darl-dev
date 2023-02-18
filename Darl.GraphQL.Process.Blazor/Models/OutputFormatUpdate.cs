using static DarlCommon.BotOutputFormat;

namespace Darl.GraphQL.Process.Blazor.Models
{
    public class OutputFormatUpdate
    {
        public bool? Hide { get; set; }
        public DisplayType? displayType { get; set; }
        public string ScoreBarColor { get; set; } = string.Empty;
        public double? ScoreBarMaxVal { get; set; }
        public double? ScoreBarMinVal { get; set; }
        public bool? Uncertainty { get; set; }
        public string ValueFormat { get; set; } = string.Empty;
        public string path { get; set; } = string.Empty;
    }
}
