using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Darl.SoftMatch
{
    interface ISoftMatch
    {
        Dictionary<string, List<string>> properNouns { get; set; }

        void CreateTree(List<KeyValuePair<string, string>> data);
        MatchResult Find(string example);
        void Flush();
        byte[] SerializeGraph();
    }
}
