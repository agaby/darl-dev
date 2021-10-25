using System.Collections.Generic;
using ProtoBuf;
using Newtonsoft.Json;

namespace Darl.Lineage
{
    [ProtoContract]
    public class LineageTemplate
    {
        public LineageTemplate()
        {
        }

        public List<List<LineageElement>> sequence { get; set; }

        [ProtoMember(1)]
        public string text { get; set; }

        [ProtoMember(2)]
        public string encSequence {
            get
            {
                return JsonConvert.SerializeObject(sequence);
            }
            set
            {
                sequence = JsonConvert.DeserializeObject<List<List<LineageElement>>>(value);
            }
        }
    }
}