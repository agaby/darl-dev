using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Darl.Thinkbase
{
    //Represents the data required to customize a generic Knowledge Graph for an individual's state.
    //Both the attributes of one or more objects can be overwritten, and the presence of a connection can be
    //defined to and from those object and remote KnowledgeStates.
    public class KnowledgeState : GraphAbstraction
    {
        public KnowledgeState()
        {
            userId = string.Empty;
            subjectId = string.Empty;
            knowledgeGraphName = string.Empty;
        }

        /// <summary>
        /// The owner of the data, a Guid
        /// </summary>
        public string userId { get; set; }

        /// <summary>
        /// the id of the subject to which this data relates 
        /// </summary>
        public string subjectId { get; set; }

        /// <summary>
        /// the KG it applies to
        /// </summary>
        public string knowledgeGraphName { get; set; }

        public DateTime? created { get; set; }

        /// <summary>
        /// If a processId is present, this data is used only for machine learning and can be deleted when that process terminates.
        /// </summary>
        public string? processId { get; set; }


        /// <summary>
        /// The data, organized as GraphObject.Id against the graphAttributes to apply.
        /// Connections can also be handled. In that case the id relates to the parent GraphConnection
        /// and the value contains the subjectId of the remote connection.
        /// Since a parent GraphObject or GraphConnection id can occur only once, the local end is always implied to be
        /// another object in this KnowledgeState
        /// </summary>
        public Dictionary<string, List<GraphAttribute>> data { get; set; } = new Dictionary<string, List<GraphAttribute>>();

        public bool ContainsRecord(string id)
        {
            return data.ContainsKey(id);
        }

        public bool ContainsAttribute(string id, string lineage)
        {
            if (data.ContainsKey(id))
                return data[id].Any(a => a.lineage == lineage && a.confidence > 0.0);
            return false;
        }

        public void AddAttribute(string id, GraphAttribute att)
        {
            if (att.confidence > 0.0) //don't load unknown atts.
            {
                if (!ContainsRecord(id))
                {
                    data.Add(id, new List<GraphAttribute> { att });
                }
                else
                {
                    var existing = data[id].FirstOrDefault(a => a.lineage.StartsWith(att.lineage));
                    if (existing != null)
                    {
                        data[id].Remove(existing);
                    }
                    data[id].Add(att);
                }
            }
        }
        public GraphAttribute? GetAttribute(string id, string lineage)
        {
            if (data.ContainsKey(id))
                return data[id].FirstOrDefault(a => a.lineage == lineage);
            return null;
        }

        public int RecordCount { get { return data.Count; } }

        public List<GraphAttributeInput> ConvertInputList(string i)
        {
            var l = new List<GraphAttributeInput>();
            foreach (var a in data[i])
            {
                l.Add(a.ConvertToInput());
            }
            return l;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is KnowledgeState))
                return false;
            var other = obj as KnowledgeState;
            if (knowledgeGraphName != other.knowledgeGraphName)
                return false;
            return subjectId == other.subjectId;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"knowledgeGraphName: {knowledgeGraphName}, ");
            sb.Append($"subjectId: {subjectId}, ");
            sb.AppendLine($"userId: {userId} ");
            if(data != null)
            {
                foreach (var s in data.Keys)
                {
                    sb.Append($"Object: {s}, Attributes: ");
                    foreach(var v in data[s])
                        sb.AppendLine(v.ToString());
                }    
            }
            return sb.ToString();
        }
    }
}
