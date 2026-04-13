/// </summary>

﻿namespace Darl.Thinkbase.Meta
{
    public class IntraSetDependency
    {
        /// Gets or sets the Dependant output.
        /// </summary>
        /// <value>
        /// The output.
        /// </value>
        public string output { get; set; }
        /// Gets or sets the depended output.
        /// </summary>
        /// <value>
        /// The output as input.
        /// </value>
        public string outputAsInput { get; set; }

        /// An external object on which this output is dependent.
        /// </summary>
        public GraphObject dependentObject { get; set; }

        /// The lineage for the attribute on which this output is dependent.
        /// </summary>
        public string attributeLineage { get; set; }

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
