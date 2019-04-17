using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Models
{
    public class MLResult
    {
        public int percentTest { get; set; }
        public double inSamplePercent { get; set; }

        public double outSamplePercent { get; set; }

        public double inSampleRMSError { get; set; }

        public double outSampleRMSError { get; set; }

        public double inSamplePercentUnknown { get; set; }

        public double outSamplePercentUnknown { get; set; }

        public DateTime executionDate { get; set; }

        public TimeSpan executionTime { get; set; }

    }
}
