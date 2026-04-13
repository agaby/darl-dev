/// </summary>

﻿using System;

namespace Darl.GraphQL.Models.Models
{
    public class MLResult
    {
        public int trainPercent { get; set; }
        public double trainPerformance { get; set; }
        public double testPerformance { get; set; }
        public double unknownResponsePercent { get; set; }
        public string code { get; set; }
        public string errorText { get; set; }

        public DateTime executionDate { get; set; }

        public TimeSpan executionTime { get; set; }

    }
}
