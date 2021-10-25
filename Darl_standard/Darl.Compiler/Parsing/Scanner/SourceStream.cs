// ***********************************************************************
// Assembly         : DarlCompiler
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="SourceStream.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using Darl_standard;

namespace DarlCompiler.Parsing
{

    /// <summary>
    /// Class SourceStream.
    /// </summary>
    public class SourceStream : ISourceStream
    {
        /// <summary>
        /// The _string comparison
        /// </summary>
        StringComparison _stringComparison;
        /// <summary>
        /// The _tab width
        /// </summary>
        int _tabWidth;
        /// <summary>
        /// The _chars
        /// </summary>
        char[] _chars;
        /// <summary>
        /// The _text length
        /// </summary>
        int _textLength;

        /// <summary>
        /// Initializes a new instance of the <see cref="SourceStream"/> class.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="caseSensitive">if set to <c>true</c> [case sensitive].</param>
        /// <param name="tabWidth">Width of the tab.</param>
        public SourceStream(string text, bool caseSensitive, int tabWidth)
            : this(text, caseSensitive, tabWidth, new SourceLocation())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SourceStream"/> class.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="caseSensitive">if set to <c>true</c> [case sensitive].</param>
        /// <param name="tabWidth">Width of the tab.</param>
        /// <param name="initialLocation">The initial location.</param>
        public SourceStream(string text, bool caseSensitive, int tabWidth, SourceLocation initialLocation)
        {
            _text = text;
            _textLength = _text.Length;
            _chars = Text.ToCharArray();
            _stringComparison = caseSensitive ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase;
            _tabWidth = tabWidth;
            _location = initialLocation;
            _previewPosition = _location.Position;
            if (_tabWidth <= 1)
                _tabWidth = 8;
        }

        #region ISourceStream Members
        /// <summary>
        /// Returns the source text
        /// </summary>
        /// <value>The text.</value>
        public string Text
        {
            get { return _text; }
            /// <summary>
            /// The _text
            /// </summary>
        }
        string _text;

        /// <summary>
        /// Gets or sets the current position in the source file. When reading the value, returns Location.Position value.
        /// When a new value is assigned, the Location is modified accordingly.
        /// </summary>
        /// <value>The position.</value>
        public int Position
        {
            get { return _location.Position; }
            set
            {
                if (_location.Position != value)
                    SetNewPosition(value);
            }
        }

        /// <summary>
        /// Gets or sets the start location (position, row, column) of the new token
        /// </summary>
        /// <value>The location.</value>
        public SourceLocation Location
        {
            [System.Diagnostics.DebuggerStepThrough]
            get { return _location; }
            set { _location = value; }
            /// <summary>
            /// The _location
            /// </summary>
        }
        SourceLocation _location;

        /// <summary>
        /// Gets or sets the current preview position in the source file. Must be greater or equal to Location.Position
        /// </summary>
        /// <value>The preview position.</value>
        public int PreviewPosition
        {
            get { return _previewPosition; }
            set { _previewPosition = value; }
            /// <summary>
            /// The _preview position
            /// </summary>
        }
        int _previewPosition;

        /// <summary>
        /// Gets a char at preview position
        /// </summary>
        /// <value>The preview character.</value>
        public char PreviewChar
        {
            [System.Diagnostics.DebuggerStepThrough]
            get
            {
                if (_previewPosition >= _textLength)
                    return '\0';
                return _chars[_previewPosition];
            }
        }

        /// <summary>
        /// Gets the char at position next after the PrevewPosition
        /// </summary>
        /// <value>The next preview character.</value>
        public char NextPreviewChar
        {
            [System.Diagnostics.DebuggerStepThrough]
            get
            {
                if (_previewPosition + 1 >= _textLength) return '\0';
                return _chars[_previewPosition + 1];
            }
        }

        /// <summary>
        /// Tries to match the symbol with the text at current preview position.
        /// </summary>
        /// <param name="symbol">A symbol to match</param>
        /// <returns>True if there is a match; otherwise, false.</returns>
        public bool MatchSymbol(string symbol)
        {
            try
            {
                int cmp = string.Compare(_text, PreviewPosition, symbol, 0, symbol.Length, _stringComparison);
                return cmp == 0;
            }
            catch
            {
                //exception may be thrown if Position + symbol.length > text.Length; 
                // this happens not often, only at the very end of the file, so we don't check this explicitly
                //but simply catch the exception and return false. Again, try/catch block has no overhead
                // if exception is not thrown. 
                return false;
            }
        }

        /// <summary>
        /// Creates a new token based on current preview position.
        /// </summary>
        /// <param name="terminal">A terminal associated with the token.</param>
        /// <returns>New token.</returns>
        public Token CreateToken(Terminal terminal)
        {
            var tokenText = GetPreviewText();
            return new Token(terminal, this.Location, tokenText, tokenText);
        }
        /// <summary>
        /// Creates a new token based on current preview position and sets its Value field.
        /// </summary>
        /// <param name="terminal">A terminal associated with the token.</param>
        /// <param name="value">The value associated with the token.</param>
        /// <returns>New token.</returns>
        public Token CreateToken(Terminal terminal, object value)
        {
            var tokenText = GetPreviewText();
            return new Token(terminal, this.Location, tokenText, value);
        }

        /// <summary>
        /// EOFs this instance.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        [System.Diagnostics.DebuggerStepThrough]
        public bool EOF()
        {
            return _previewPosition >= _textLength;
        }
        #endregion

        //returns substring from Location.Position till (PreviewPosition - 1)
        /// <summary>
        /// Gets the preview text.
        /// </summary>
        /// <returns>System.String.</returns>
        private string GetPreviewText()
        {
            var until = _previewPosition;
            if (until > _textLength) until = _textLength;
            var p = _location.Position;
            string text = Text.Substring(p, until - p);
            return text;
        }

        // To make debugging easier: show 20 chars from current position
        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            string result;
            try
            {
                var p = Location.Position;
                if (p + 20 < _textLength)
                    result = _text.Substring(p, 20) + Resources.LabelSrcHaveMore;// " ..."
                else
                    result = _text.Substring(p) + Resources.LabelEofMark; //"(EOF)"
            }
            catch (Exception)
            {
                result = PreviewChar + Resources.LabelSrcHaveMore;
            }
            return string.Format(Resources.MsgSrcPosToString, result, Location); //"[{0}], at {1}"
        }

        //Computes the Location info (line, col) for a new source position.
        /// <summary>
        /// Sets the new position.
        /// </summary>
        /// <param name="newPosition">The new position.</param>
        /// <exception cref="System.Exception"></exception>
        private void SetNewPosition(int newPosition)
        {
            if (newPosition < Position)
                throw new Exception(Resources.ErrCannotMoveBackInSource);
            int p = Position;
            int col = Location.Column;
            int line = Location.Line;
            while (p < newPosition)
            {
                if (p >= _textLength)
                    break;
                var curr = _chars[p];
                switch (curr)
                {
                    case '\n': line++; col = 0; break;
                    case '\r': break;
                    case '\t': col = (col / _tabWidth + 1) * _tabWidth; break;
                    default: col++; break;
                } //switch
                p++;
            }
            Location = new SourceLocation(p, line, col);
        }


    }

}
