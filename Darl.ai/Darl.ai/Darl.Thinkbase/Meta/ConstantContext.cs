/// </summary>

﻿using DarlCompiler.Ast;
using System.Collections.Generic;

namespace Darl.Thinkbase.Meta
{
    public class ConstantContext
    {
        /// Gets or sets the outputs.
        /// </summary>
        /// <value>
        /// The outputs.
        /// </value>
        public Dictionary<string, IOSequenceDefinitionNode> outputs { get; set; }

        /// Gets or sets the inputs.
        /// </summary>
        /// <value>
        /// The inputs.
        /// </value>
        public Dictionary<string, InputDefinitionNode> inputs { get; set; }

        /// Gets or sets the strings.
        /// </summary>
        /// <value>
        /// The strings.
        /// </value>
        public Dictionary<string, StringDefinitionNode> strings { get; set; }

        /// Gets or sets the constants.
        /// </summary>
        /// <value>
        /// The constants.
        /// </value>
        public Dictionary<string, ConstantDefinitionNode> constants { get; set; }

        /// Gets or sets the periods.
        /// </summary>
        /// <value>
        /// The constants.
        /// </value>
        public Dictionary<string, DurationDefinitionNode> durations { get; set; }

        /// Gets or sets the lineages
        /// </summary>
        public Dictionary<string, LineageDefinitionNode> lineages { get; set; }


        /// Gets or sets the stores.
        /// </summary>
        /// <value>
        /// The stores.
        /// </value>
        public Dictionary<string, StoreNode> storeInputs { get; set; }

        /// Gets or sets the store addresses used as outputs.
        /// </summary>
        /// <value>
        /// The store addresses used as outputs.
        /// </value>
        public Dictionary<string, StoreNode> storeOutputs { get; set; }


        /// Gets or sets the store definitions.
        /// </summary>
        /// <value>
        /// The store definitions.
        /// </value>
        public Dictionary<string, StoreDefinitionNode> stores { get; set; }


        /// The IO for a set or category on  the other side of the nearest "is" statement.
        /// </summary>
        public string controllingIO { get; set; }

        public AstContext parseContext { get; set; }

    }
}
