/// </summary>

﻿using Darl.Common;

namespace Darl.Thinkbase
{
    public class ModelMetaData
    {
        public string? description { get; set; }
        public string? initialText { get; set; }
        public string? author { get; set; }
        public string? copyright { get; set; }
        public string? licenseUrl { get; set; }
        public IGraphModel.DateDisplay? dateDisplay { get; set; }
        public IGraphModel.InferenceTime? inferenceTime { get; set; }
        public DarlTime? fixedTime { get; set; }
        public string? defaultTarget { get; set; }
    }
}
