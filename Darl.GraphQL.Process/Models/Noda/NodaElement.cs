using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Models.Noda
{
    public abstract class NodaElement
    {
        public string? kind { get; set; }

        public NodaTone tone { get; set; }
        public bool folded { get; set; } = false;

        public string uuid { get; set; } = Guid.NewGuid().ToString();

        public double size { get; set; }

        public List<NodaProperty> properties { get; set; } = new List<NodaProperty>();
    }
}
