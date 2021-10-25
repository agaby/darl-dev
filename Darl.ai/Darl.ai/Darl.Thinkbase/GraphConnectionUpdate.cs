using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.Thinkbase
{
    public class GraphConnectionUpdate : GraphConnectionInput
    {
        public string id { get; set; }

        public bool? inferred { get; set; }

    }
}
