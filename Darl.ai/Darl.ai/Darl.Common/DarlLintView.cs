namespace DarlCommon
{
    public class DarlLintView
    {
        /// <summary>
        /// Gets or sets the line_no.
        /// </summary>
        /// <value>The line_no.</value>
        public int line_no { get; set; }
        /// <summary>
        /// Gets or sets the column_no_start.
        /// </summary>
        /// <value>The column_no_start.</value>
        public int column_no_start { get; set; }
        /// <summary>
        /// Gets or sets the column_no_stop.
        /// </summary>
        /// <value>The column_no_stop.</value>
        public int column_no_stop { get; set; }
        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>The message.</value>
        public string message { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the severity.
        /// </summary>
        /// <value>The severity.</value>
        /// <remarks>choices are "warning", "error"</remarks>
        public string severity { get; set; }
    }
}
