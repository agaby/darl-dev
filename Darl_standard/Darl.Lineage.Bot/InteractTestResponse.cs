using DarlCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Darl.Lineage.Bot
{
    public class InteractTestResponse
    {
        public DarlVar response { get; set; }

        public string darl { get; set; }

        public List<MatchedElement> matches { get; set; }

        public string reference { get; set; }

        public List<string> activeNodes { get; set; }
    }
}
