/// <summary>
/// </summary>

﻿using System.Collections.Generic;

namespace DarlLanguage.Processing
{
    /// <summary>
    /// Processes and matches sequences
    /// </summary>
    /// <typeparam name="T">The kind of sequenced entity</typeparam>
    public class Sequence<T>
    {
        /// <summary>
        /// Finds a match in the source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="sequence">The sequence.</param>
        /// <param name="comparer">The comparer.</param>
        /// <returns>True if a match is found.</returns>
        public bool Find(List<List<T>> source, List<List<T>> sequence, IComparer<T> comparer)
        {
            for (int n = 0; n < (source.Count - sequence.Count) + 1; n++)
                if (RecursivelyFind(source, sequence, comparer, n, 0))
                    return true;
            return false;

        }

        /// <summary>
        /// Recursively match the sequence.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="sequence">The sequence.</param>
        /// <param name="comparer">The comparer.</param>
        /// <param name="offset">The offset into the source</param>
        /// <param name="depth">The depth of match in the sequence.</param>
        /// <returns>
        /// True if a match is found.
        /// </returns>
        private bool RecursivelyFind(List<List<T>> source, List<List<T>> sequence, IComparer<T> comparer, int offset, int depth)
        {
            if (offset >= source.Count)
                return false;
            foreach (var seq in sequence[depth])
            {
                foreach (var sr in source[offset])
                {
                    if (comparer.Compare(seq, sr) == 0)
                    {
                        if (depth + 1 == sequence.Count)
                            return true;
                        if (RecursivelyFind(source, sequence, comparer, offset + 1, depth + 1))
                            return true;
                    }
                    else if (depth > 0)
                    {
                        if (RecursivelyFind(source, sequence, comparer, offset + 1, depth))
                            return true;
                    }
                }
            }
            return false;
        }
    }
}
