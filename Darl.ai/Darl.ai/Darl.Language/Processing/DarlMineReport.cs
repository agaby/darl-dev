/// </summary>

﻿namespace DarlLanguage.Processing
{
    /// Mining performance class
    /// </summary>
    public class DarlMineReport
    {
        /// Gets the train percent.
        /// </summary>
        /// <value>
        /// The train percent.
        /// </value>
        public int trainPercent { get; internal set; }
        /// Gets the train performance.
        /// </summary>
        /// <value>
        /// The train performance.
        /// </value>
        public double trainPerformance { get; internal set; }
        /// Gets the test performance.
        /// </summary>
        /// <value>
        /// The test performance.
        /// </value>
        public double testPerformance { get; internal set; }

        /// Gets the unknown response percent.
        /// </summary>
        /// <value>
        /// The unknown response percent.
        /// </value>
        public double unknownResponsePercent { get; internal set; }

        public string code { get; set; }

        public string errorText { get; set; }
    }
}
