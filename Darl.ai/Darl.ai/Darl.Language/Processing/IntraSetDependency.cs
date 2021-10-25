namespace DarlLanguage.Processing
{
    /// <summary>
    /// Documents a dependency between an output within a ruleset and another output.
    /// </summary>
    public class IntraSetDependency
    {
        /// <summary>
        /// Gets or sets the dependant output.
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
