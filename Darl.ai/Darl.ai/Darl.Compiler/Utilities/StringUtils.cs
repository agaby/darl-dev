// ***********************************************************************
// Assembly         : DarlCompiler
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="StringUtils.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;

/// <summary>
/// The DarlCompiler namespace.
/// </summary>
namespace DarlCompiler
{

    /// <summary>
    /// Class Strings.
    /// </summary>
    public static class Strings
    {
        /// <summary>
        /// All latin letters
        /// </summary>
        public const string AllLatinLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        /// <summary>
        /// The decimal digits
        /// </summary>
        public const string DecimalDigits = "1234567890";
        /// <summary>
        /// The octal digits
        /// </summary>
        public const string OctalDigits = "12345670";
        /// <summary>
        /// The hexadecimal digits
        /// </summary>
        public const string HexDigits = "1234567890aAbBcCdDeEfF";
        /// <summary>
        /// The binary digits
        /// </summary>
        public const string BinaryDigits = "01";

        /// <summary>
        /// Joins the strings.
        /// </summary>
        /// <param name="separator">The separator.</param>
        /// <param name="values">The values.</param>
        /// <returns>System.String.</returns>
        public static string JoinStrings(string separator, IEnumerable<string> values)
        {
            StringList list = new StringList();
            list.AddRange(values);
            string[] arr = new string[list.Count];
            list.CopyTo(arr, 0);
            return string.Join(separator, arr);
        }

    }

    /// <summary>
    /// Class StringDictionary.
    /// </summary>
    [Serializable]
    public class StringDictionary : Dictionary<string, string> { }
    /// <summary>
    /// Class CharList.
    /// </summary>
    public class CharList : List<char> { }

    // CharHashSet: adding Hash to the name to avoid confusion with System.Runtime.Interoperability.CharSet
    // Adding case sensitivity
    /// <summary>
    /// Class CharHashSet.
    /// </summary>
    [Serializable]
    public class CharHashSet : HashSet<char>
    {
        /// <summary>
        /// The _case sensitive
        /// </summary>
        readonly bool _caseSensitive;
        /// <summary>
        /// Initializes a new instance of the <see cref="CharHashSet"/> class.
        /// </summary>
        /// <param name="caseSensitive">if set to <c>true</c> [case sensitive].</param>
        public CharHashSet(bool caseSensitive = true)
        {
            _caseSensitive = caseSensitive;
        }
        /// <summary>
        /// Adds the specified ch.
        /// </summary>
        /// <param name="ch">The ch.</param>
        public new void Add(char ch)
        {
            if (_caseSensitive)
                base.Add(ch);
            else
            {
                base.Add(char.ToLowerInvariant(ch));
                base.Add(char.ToUpperInvariant(ch));
            }

        }
    }

    /// <summary>
    /// Class TypeList.
    /// </summary>
    public class TypeList : List<Type>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TypeList"/> class.
        /// </summary>
        public TypeList() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="TypeList"/> class.
        /// </summary>
        /// <param name="types">The types.</param>
        public TypeList(params Type[] types) : base(types) { }
    }


    /// <summary>
    /// Class StringSet.
    /// </summary>
    [Serializable]
    public class StringSet : HashSet<string>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StringSet"/> class.
        /// </summary>
        public StringSet() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="StringSet"/> class.
        /// </summary>
        /// <param name="comparer">The comparer.</param>
        public StringSet(StringComparer comparer) : base(comparer) { }
        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return ToString(" ");
        }
        /// <summary>
        /// Adds the range.
        /// </summary>
        /// <param name="items">The items.</param>
        public void AddRange(params string[] items)
        {
            base.UnionWith(items);
        }
        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <param name="separator">The separator.</param>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public string ToString(string separator)
        {
            return Strings.JoinStrings(separator, this);
        }
    }

    /// <summary>
    /// Class StringList.
    /// </summary>
    public class StringList : List<string>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StringList"/> class.
        /// </summary>
        public StringList() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="StringList"/> class.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public StringList(params string[] args)
        {
            AddRange(args);
        }
        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return ToString(" ");
        }
        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <param name="separator">The separator.</param>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public string ToString(string separator)
        {
            return Strings.JoinStrings(separator, this);
        }
        //Used in sorting suffixes and prefixes; longer strings must come first in sort order
        /// <summary>
        /// Longers the first.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns>System.Int32.</returns>
        public static int LongerFirst(string x, string y)
        {
            try
            {//in case any of them is null
                if (x.Length > y.Length) return -1;
            }
            catch { }
            if (x == y) return 0;
            return 1;
        }
    }
}
