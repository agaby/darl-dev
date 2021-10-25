using System.Collections.Generic;

namespace DarlLanguage.Processing
{
    /// <summary>
    /// Categories, sets, string constants, and numeric constants are all immutable. so can be set up at parse time.
    /// This class holds the data to make that possible.
    /// </summary>
    public class ConstantContext
    {
        /// <summary>
        /// Gets or sets the outputs.
        /// </summary>
        /// <value>
        /// The outputs.
        /// </value>
        public Dictionary<string, IOSequenceDefinitionNode> outputs { get; set; }

        /// <summary>
        /// Gets or sets the inputs.
        /// </summary>
        /// <value>
        /// The inputs.
        /// </value>
        public Dictionary<string, InputDefinitionNode> inputs { get; set; }

        /// <summary>
        /// Gets or sets the strings.
        /// </summary>
        /// <value>
        /// The strings.
        /// </value>
        public Dictionary<string, StringDefinitionNode> strings { get; set; }

        /// <summary>
        /// Gets or sets the constants.
        /// </summary>
        /// <value>
        /// The constants.
        /// </value>
        public Dictionary<string, ConstantDefinitionNode> constants { get; set; }

        /// <summary>
        /// Gets or sets the periods.
        /// </summary>
        /// <value>
        /// The constants.
        /// </value>
        public Dictionary<string, DurationDefinitionNode> durations { get; set; }

        /// <summary>
        /// Gets or sets the sequences.
        /// </summary>
        /// <value>
        /// The sequences.
        /// </value>
        public Dictionary<string, SequenceDefinitionNode> sequences { get; set; }

        /// <summary>
        /// Gets or sets the stores.
        /// </summary>
        /// <value>
        /// The stores.
        /// </value>
        public Dictionary<string, StoreNode> storeInputs { get; set; }

        /// <summary>
        /// Gets or sets the store addresses used as outputs.
        /// </summary>
        /// <value>
        /// The store addresses used as outputs.
        /// </value>
        public Dictionary<string, StoreNode> storeOutputs { get; set; }


        /// <summary>
        /// Gets or sets the store definitions.
        /// </summary>
        /// <value>
        /// The store definitions.
        /// </value>
        public Dictionary<string, StoreDefinitionNode> stores { get; set; }


        /// <summary>
        /// The IO for a set or category on  the other side of the nearest "is" statement.
        /// </summary>
        public string controllingIO { get; set; }

    }
}
