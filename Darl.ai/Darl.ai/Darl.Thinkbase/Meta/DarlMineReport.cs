using static Darl.Thinkbase.IGraphHandler;

namespace Darl.Thinkbase.Meta
{
    /// <summary>
    /// Mining performance class
    /// </summary>
    public class DarlMineReport
    {
        /// <summary>
        /// Gets the train percent.
        /// </summary>
        /// <value>
        /// The train percent.
        /// </value>
        public int trainPercent { get; internal set; }
        /// <summary>
        /// Gets the train performance.
        /// </summary>
        /// <value>
        /// The train performance.
        /// </value>
        public double trainPerformance { get; internal set; }
        /// <summary>
        /// Gets the test performance.
        /// </summary>
        /// <value>
        /// The test performance.
        /// </value>
        public double testPerformance { get; internal set; }

        /// <summary>
        /// Gets the unknown response percent.
        /// </summary>
        /// <value>
        /// The unknown response percent.
        /// </value>
        public double unknownResponsePercent { get; internal set; }

        public string? code { get; set; }

        public string? errorText { get; set; }

        public SetChoices sets { get; set; } = SetChoices.three;
    }
}
