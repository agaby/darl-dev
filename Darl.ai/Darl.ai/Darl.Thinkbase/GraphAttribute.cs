using DarlCommon;
using ProtoBuf;
using System;
using System.Text;

namespace Darl.Thinkbase
{
    [ProtoContract(AsReferenceDefault = true)]
    public class GraphAttribute : GraphElement
    {

        public enum DataType
        {
            /// <summary>
            /// Numeric including fuzzy
            /// </summary>
            numeric,
            /// <summary>
            /// One or more categories with confidences
            /// </summary>
            categorical,
            /// <summary>
            /// Textual
            /// </summary>
            textual,
            /// <summary>
            /// a text sequence
            /// </summary>
            sequence,
            /// <summary>
            /// A time value
            /// </summary>
            temporal,
            /// <summary>
            /// A time period
            /// </summary>
            duration,
            /// <summary>
            /// Text in markdown format
            /// </summary>
            markdown,
            /// <summary>
            /// A rule set
            /// </summary>
            ruleset,
            /// <summary>
            /// A link to another attribute in another kg
            /// </summary>
            link,
            /// <summary>
            /// For use only in KStates. The endpoint subjectId resides in value
            /// </summary>
            connection

        }

        [ProtoMember(1)]
        public string value { get; set; }

        [ProtoMember(2)]
        public double confidence { get; set; }

        [ProtoMember(3)]
        public DataType type { get; set; }

        public DarlVar Convert()
        {
            return new DarlVar { Value = value, name = "attribute", unknown = confidence == 0.0, weight = confidence, dataType = ConvertDataType() };
        }

        public DarlVar.DataType ConvertDataType()
        {
            DarlVar.DataType dataType = DarlVar.DataType.textual;
            Enum.TryParse<DarlVar.DataType>(type.ToString(), out dataType);
            return dataType;
        }

        public GraphAttributeInput ConvertToInput()
        {
            return new GraphAttributeInput { name = name, confidence = confidence, existence = existence, inferred = inferred, lineage = lineage, type = type, value = value };
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            var truncValue = value.Length > 10 ? (value.Substring(0, 10) + "...") : value;
            sb.AppendLine($"Name = {name},");
            sb.AppendLine($"lineage = {lineage},");
            sb.AppendLine($"type = {type},");
            sb.AppendLine($"confidence = {confidence},");
            sb.AppendLine($"value = {truncValue},");
            return sb.ToString();
        }

    }
}
