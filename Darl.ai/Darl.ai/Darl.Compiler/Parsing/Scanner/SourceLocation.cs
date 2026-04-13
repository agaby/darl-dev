/// </summary>

﻿// ***********************************************************************
// Assembly         : DarlCompiler
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="SourceLocation.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using Darl.ai;

namespace DarlCompiler.Parsing
{

    /// Struct SourceLocation
    /// </summary>
    public struct SourceLocation
    {
        /// The position
        /// </summary>
        public int Position;
        /// Source line number, 0-based.
        /// </summary>
        public int Line;
        /// Source column number, 0-based.
        /// </summary>
        public int Column;
        /// Initializes a new instance of the <see cref="SourceLocation"/> struct.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="line">The line.</param>
        /// <param name="column">The column.</param>
        public SourceLocation(int position, int line, int column)
        {
            Position = position;
            Line = line;
            Column = column;
        }
        //Line/col are zero-based internally
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return string.Format(Resources.FmtRowCol, Line + 1, Column + 1);
        }
        //Line and Column displayed to user should be 1-based
        /// To the UI string.
        /// </summary>
        /// <returns>System.String.</returns>
        public string ToUiString()
        {
            return string.Format(Resources.FmtRowCol, Line + 1, Column + 1);
        }
        /// Compares the specified x.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns>System.Int32.</returns>
        public static int Compare(SourceLocation x, SourceLocation y)
        {
            if (x.Position < y.Position) return -1;
            if (x.Position == y.Position) return 0;
            return 1;
        }
        /// Gets the empty.
        /// </summary>
        /// <value>The empty.</value>
        public static SourceLocation Empty
        {
            get { return _empty; }
            /// The _empty
            /// </summary>
        }
        static SourceLocation _empty = new SourceLocation();

        /// Implements the +.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns>The result of the operator.</returns>
        public static SourceLocation operator +(SourceLocation x, SourceLocation y)
        {
            return new SourceLocation(x.Position + y.Position, x.Line + y.Line, x.Column + y.Column);
        }
        /// Implements the +.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="offset">The offset.</param>
        /// <returns>The result of the operator.</returns>
        public static SourceLocation operator +(SourceLocation x, int offset)
        {
            return new SourceLocation(x.Position + offset, x.Line, x.Column + offset);
        }
    }//SourceLocation

    /// Struct SourceSpan
    /// </summary>
    public struct SourceSpan
    {
        /// The location
        /// </summary>
        public readonly SourceLocation Location;
        /// The length
        /// </summary>
        public readonly int Length;
        /// Initializes a new instance of the <see cref="SourceSpan"/> struct.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="length">The length.</param>
        public SourceSpan(SourceLocation location, int length)
        {
            Location = location;
            Length = length;
        }
        /// Gets the end position.
        /// </summary>
        /// <value>The end position.</value>
        public int EndPosition
        {
            get { return Location.Position + Length; }
        }
        /// Ins the range.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool InRange(int position)
        {
            return (position >= Location.Position && position <= EndPosition);
        }

    }


}
