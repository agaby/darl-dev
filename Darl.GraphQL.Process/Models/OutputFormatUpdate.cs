/// <summary>
/// </summary>

﻿using static DarlCommon.BotOutputFormat;

namespace Darl.GraphQL.Models.Models
{
    public class OutputFormatUpdate
    {
        public bool? Hide { get; set; }
        public DisplayType? displayType { get; set; }
        public string ScoreBarColor { get; set; }
        public double? ScoreBarMaxVal { get; set; }
        public double? ScoreBarMinVal { get; set; }
        public bool? Uncertainty { get; set; }
        public string ValueFormat { get; set; }
        public string path { get; set; }
    }
}
