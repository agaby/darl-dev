using Darl.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Darl.Thinkbase
{
    public class KnowledgeRecordInput : KnowledgeStateInput
    {
        /// <summary>
        /// Add an attribute reference to a knowledge record
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <remarks>To Do: Add integrity checks on categories, ranges, etc</remarks>
        public void AddReference(GraphObject parent, string name, string value)
        {
            bool found = false;
            if (parent.properties != null)
            {
                foreach (var l in parent.properties)
                {
                    if (l.name == name)
                    {
                        if (!data.Any(a => a.name == parent.id ))
                        {
                            data.Add(new StringListGraphAttributeInputPair { name = parent.id ?? string.Empty, value = new List<GraphAttributeInput>() });
                        }
                        var att = data.First(a => a.name == (parent.id ?? string.Empty)).value.FirstOrDefault(a => a.name == name);
                        if (att == null)
                        {
                            data.First(a => a.name == (parent.id ?? string.Empty)).value.Add(new GraphAttributeInput { value = value, type = l.type, lineage = l.lineage ?? string.Empty, confidence = l.confidence, name = l.name, inferred = true });
                        }
                        else
                        {
                            att.value = value;
                        }
                        found = true;
                        break;
                    }
                }
            }
            if (!found)
            {
                throw new Exception($"Attribute {name} not found. Schema change?");
            }
        }

        /// <summary>
        /// Add existence to a knowledge state.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="existence"></param>
        public void AddExistence(GraphObject parent, List<DarlTime?> existence)
        {
            if (!data.Any(a => a.name == parent.id))
            {
                data.Add(new StringListGraphAttributeInputPair { name = parent.id ?? string.Empty, value = new List<GraphAttributeInput>() });
            }
            var att = data.First(a => a.name == (parent.id ?? string.Empty)).value.FirstOrDefault(a => a.name == "existence");
            if (att == null)
            {
                data.First(a => a.name == (parent.id ?? string.Empty)).value.Add(new GraphAttributeInput { type = GraphAttribute.DataType.temporal, lineage = "noun:01,5,03,3,018", confidence = 1.0, name = "existence", inferred = true, existence = existence });
            }
            else
            {
                att.existence = existence;
            }
        }

        public void AddConnection(GraphConnection link, string remoteId, List<DarlTime>? existence = null)
        {
            if (!data.Any(a => a.name == (link.id ?? string.Empty)))
            {
                data.Add(new StringListGraphAttributeInputPair { name = (link.id ?? string.Empty), value = new List<GraphAttributeInput>() });
            }
            data.First(a => a.name == (link.id ?? string.Empty)).value.Add(new GraphAttributeInput { name = (link.name ?? string.Empty), type = GraphAttribute.DataType.connection, confidence = link.weight, lineage = link.lineage ?? string.Empty, inferred = true, value = remoteId, existence = existence});
        }
    }
}
