/// <summary>
/// </summary>

﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Datl.Language
{
    public class TextProcess : DatlProcess
    {
        private readonly StringBuilder sb = new StringBuilder();
        public override dynamic CreateDest(dynamic source)
        {
            sb.Clear();
            return source;
        }

        public override string ExtractVariableName(dynamic block)
        {
            var m = block as Match;
            var s = m.Value;
            return ExtractVariableName(s);
        }

        public override List<dynamic> FindBlocks(dynamic source)
        {
            var r = new Regex(blockRegex);
            var s = source as string;
            var m = r.Matches(s);
            var l = new List<dynamic>();
            for (int n = 0; n < m.Count; n++)
                l.Add(m[n]);
            return l;
        }

        public override bool IsEndBlock(dynamic block)
        {
            var m = block as Match;
            return m.Value.Contains(endBlockTag);
        }

        public override bool IsReplaceBlock(dynamic block)
        {
            var m = block as Match;
            return !m.Value.Contains(startBlockTag) && !m.Value.Contains(endBlockTag);
        }

        public override bool IsStartBlock(dynamic block)
        {
            var m = block as Match;
            return m.Value.Contains(startBlockTag);
        }

        public override dynamic PostProcess(dynamic source)
        {
            return sb.ToString();
        }

        public override void ReplaceBlock(dynamic output, dynamic block, string value)
        {
            sb.Append(value);
        }

        public override void WriteSection(dynamic output, dynamic startblock, dynamic endblock)
        {
            var s = output as string;
            if (startblock == null)
            {
                if (endblock != null)
                {
                    var m = endblock as Match;
                    sb.Append(s.Substring(0, m.Index));
                }
                else
                {
                    sb.Append(s);
                }
            }
            else if (endblock == null)
            {
                var m = startblock as Match;
                sb.Append(s.Substring(m.Index + m.Length));
            }
            else
            {
                var m = startblock as Match;
                var e = endblock as Match;
                int start = m.Index + m.Length;
                sb.Append(s.Substring(start, e.Index - start));
            }

        }
        public override void ReportError(dynamic block, string error)
        {
            var m = block as Match;
            throw new Exception($"Error: {error} Location = {m.Index}");
        }
    }
}
