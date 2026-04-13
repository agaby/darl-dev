/// <summary>
/// SequenceComparer.cs - Core module for the Darl.dev project.
/// </summary>

﻿using System.Collections.Generic;

namespace DarlLanguage.Processing
{
    class SequenceComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            if (x.StartsWith(y))
                return 0;
            return 1;
        }
    }
}
