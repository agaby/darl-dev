using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Models.Noda
{
    public class NodaProperty
    {
        public string uuid { get; set; }
        public string name { get; set; }
        public string text { get; set; }
        public string image { get; set; }
        public string video { get; set; }
        public NodaTone tone { get; set; }
        public double size { get; set; }
        public string page { get; set; }
        public string notes { get; set; }
    }
}
