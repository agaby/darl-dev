using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.Lineage
{
    /// <summary>
    /// The darl code and implications that can be associated with a LineageMatchNode
    /// </summary>
    [ProtoContract()]
    public class LineageAnnotationNode : IComparable<LineageAnnotationNode>
    {
        /// <summary>
        /// Darl code
        /// </summary>
        [ProtoMember(1)]
        public List<string> darl { get; set; } = new List<string>();
        /// <summary>
        /// implications
        /// </summary>
        [ProtoMember(2)]
        public List<string> implications { get; set; } = new List<string>();

        /// <summary>
        /// List of roles with access
        /// </summary>
        /// <remarks>Empty list grants anonymous access only</remarks>
        [ProtoMember(3)]
        public List<string> accessRoles { get; set; } = new List<string>();

        /// <summary>
        /// Compares two LineageAnnotationNode
        /// </summary>
        /// <param name="other">The other node</param>
        /// <returns>their order</returns>
        public int CompareTo(LineageAnnotationNode other)
        {
            return GetCharacteristicString().CompareTo(other.GetCharacteristicString());
        }

        private string GetCharacteristicString()
        {
            var sb = new StringBuilder();
            foreach (var v in darl)
                sb.Append(v);
            foreach (var v in implications)
                sb.Append(v);
            foreach (var v in accessRoles)
                sb.Append(v);
            return sb.ToString();
        }

        public override int GetHashCode()
        {
            return GetCharacteristicString().GetHashCode();
        }
    }
}
