namespace Darl.Thinkbase.Meta
{
    public class IntraSetDependency
    {
        /// <summary>
        /// Gets or sets the Dependant output.
        /// </summary>
        /// <value>
        /// The output.
        /// </value>
        public string output { get; set; }
        /// <summary>
        /// Gets or sets the depended output.
        /// </summary>
        /// <value>
        /// The output as input.
        /// </value>
        public string outputAsInput { get; set; }

        /// <summary>
        /// An external object on which this output is dependent.
        /// </summary>
        public GraphObject dependentObject { get; set; }

        /// <summary>
        /// The lineage for the attribute on which this output is dependent.
        /// </summary>
        public string attributeLineage { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format("{0} -> {1}", outputAsInput, output);
        }
    }
}
