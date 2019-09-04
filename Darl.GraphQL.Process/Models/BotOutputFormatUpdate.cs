using System;
using System.Collections.Generic;
using System.Text;
using static DarlCommon.BotOutputFormat;

namespace Darl.GraphQL.Models.Models
{
    public class BotOutputFormatUpdate
    {
        public DisplayType? displayType { get; set; }
        public string ValueFormat { get; set; }
      
    }
}
