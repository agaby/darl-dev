using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Darl.GraphQL.Process.Web.Models.Noda
{
    public class NodaViewLinkProps : ILayoutLink
    {
        public enum NodaViewLinkShape { Solid, Dash, Arrows}
        public string uuid { get; set; } = Guid.NewGuid().ToString();
        public string fromUuid { get; set; } = string.Empty;
        public string toUuid { get; set; } = string.Empty;
        public string title { get; set; } = string.Empty;
        public string color { get; set; } = "#000000";
        public NodaViewLinkShape shape { get; set; } = NodaViewLinkShape.Solid;
        public int size { get; set; } = 1; //1-9
        public bool selected { get; set; } = false;
        [JsonIgnore]
        public double length { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string FromNode()
        {
            throw new NotImplementedException();
        }

        public string ToNode()
        {
            throw new NotImplementedException();
        }
    }
}
