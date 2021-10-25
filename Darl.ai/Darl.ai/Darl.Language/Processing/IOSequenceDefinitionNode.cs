using DarlCompiler.Interpreter;
using System;

namespace DarlLanguage.Processing
{
    public class IOSequenceDefinitionNode : IODefinitionNode, IComparable<IOSequenceDefinitionNode>
    {
        /// <summary>
        /// Gets or sets the sequence.
        /// </summary>
        /// <value>
        /// The sequence.
        /// </value>
        public int sequence { get; set; }

        /// <summary>
        /// Gets or sets the result.
        /// </summary>
        /// <value>
        /// The result.
        /// </value>
        public DarlResult result
        {
            get
            {
                return _result;
            }
            set
            {
                _result = value;
            }
        }
        private DarlResult _result;


        protected Binding _accessor;

        /// <summary>
        /// Gets the fuzzy results.
        /// </summary>
        /// <value>
        /// The fuzzy results.
        /// </value>
        public DarlResult fuzzyResults { get; internal set; }

        /// <summary>
        /// Gets the confidence.
        /// </summary>
        /// <value>
        /// The confidence.
        /// </value>
        public double confidence { get; internal set; }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public virtual object Value { get; internal set; } = new DarlResult(0.0, true);

        /// <summary>
        /// Compares the current object with another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has the following meanings: Value Meaning Less than zero This object is less than the <paramref name="other" /> parameter.Zero This object is equal to <paramref name="other" />. Greater than zero This object is greater than <paramref name="other" />.
        /// </returns>
        public int CompareTo(IOSequenceDefinitionNode other)
        {
            return sequence.CompareTo(other.sequence);
        }

    }
}
