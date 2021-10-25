// ***********************************************************************
// Assembly         : DaslLanguage
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-26-2015
// ***********************************************************************
// <copyright file="DaslVertex.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace DaslLanguage
{
    /// <summary>
    /// Enum VertexType
    /// </summary>
    public enum VertexType
    {
        observable,
        delay,
        ruleset
    }
    /// <summary>
    /// Class DaslVertex.
    /// </summary>
    public class DaslVertex
    {
        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public string ID { get; private set; }

        /// <summary>
        /// Gets the delay value.
        /// </summary>
        /// <value>The delay value.</value>
        public string DelayValue { get; private set; }
        /// <summary>
        /// Gets a value indicating whether this instance is observable.
        /// </summary>
        /// <value><c>true</c> if this instance is observable; otherwise, <c>false</c>.</value>
        public bool IsObservable { get {  return vtype == VertexType.observable;} }
        /// <summary>
        /// Gets a value indicating whether this instance is delay.
        /// </summary>
        /// <value><c>true</c> if this instance is delay; otherwise, <c>false</c>.</value>
        public bool IsDelay { get { return vtype == VertexType.delay; } }
        /// <summary>
        /// Gets a value indicating whether this instance is rule set.
        /// </summary>
        /// <value><c>true</c> if this instance is rule set; otherwise, <c>false</c>.</value>
        public bool IsRuleSet { get { return vtype == VertexType.ruleset; } }

        /// <summary>
        /// The vtype
        /// </summary>
        private VertexType vtype;

        /// <summary>
        /// Initializes a new instance of the <see cref="DaslVertex"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="VType">Type of the v.</param>
        /// <param name="delayValue">The delay value.</param>
        public DaslVertex(string id, VertexType VType, int delayValue = 0)
        {
            ID = id;
            vtype = VType;
            DelayValue = delayValue.ToString();
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return string.Format("{0}-{1}", ID, vtype.ToString());
        }
    }
}
