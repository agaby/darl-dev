/// </summary>

﻿using System.Collections.Generic;
using System.Linq;

namespace Darl.Lineage
{
    public class LineageComparer : IComparer<string>
    {
        public bool lineageMatch { get; set; } = false;
        public bool multiMatch { get; set; } = false;
        public int Compare(string x, string y)
        {
            if (!lineageMatch && !multiMatch)
                return x.CompareTo(y);
            if (lineageMatch)
            {
                if (x.Contains(":") && y.Contains(":"))
                {
                    if (y.StartsWith(x))
                        return 0; //hierarchical match of concepts
                }
            }
            if (multiMatch)
            {
                if (x.Contains("|") && !y.Contains("|"))
                {
                    foreach (var s in x.Split('|'))
                    {
                        if (y == s)
                            return 0; //match to multiple 
                    }
                }
                else if (!x.Contains("|") && y.Contains("|"))
                {
                    foreach (var s in y.Split('|'))
                    {
                        if (x == s)
                            return 0; //match to multiple 
                    }
                }
                else if (x.Contains("|") && y.Contains("|"))
                {
                    if (x.Split('|').Intersect(y.Split('|')).Any())
                    {
                        return 0;
                    }
                }
            }
            return x.CompareTo(y);
        }
    }
}
