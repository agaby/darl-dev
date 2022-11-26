using Darl.Thinkbase.Meta;
using DarlCommon;
using System.Collections.Generic;
using System.Text;

namespace Darl.Lineage.Bot
{
    public class InteractTestResponse
    {
        public DarlVar response { get; set; }

        public string darl { get; set; }

        public List<MatchedElement> matches { get; set; }

        public string reference { get; set; }

        public List<string> activeNodes { get; set; }

        public DarlMetaActivity? codeActivity { get; set; }

        /// <summary>
        /// Add highlighting to the darl code using markdown based on the codeActivity list
        /// </summary>
        /// <returns>A markdown string</returns>
        public string FormatDarl()
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(darl))
            {
                if (codeActivity == null)
                {
                    sb.Append(darl);
                }
                else
                {
                    var highLightMap = new bool[darl.Length];
                    foreach (var c in codeActivity.activeNodes)
                    {
                        var start = c.location.Location.Position;
                        for (int i = start; i < start + c.location.Length; i++)
                        {
                            highLightMap[i] = true;
                        }
                    }
                    bool highlighted = false;
                    for (int i = 0; i < darl.Length; i++)
                    {
                        if (highlighted != highLightMap[i])
                        {
                            sb.Append("__");
                            highlighted = highLightMap[i];
                        }
                        sb.Append(darl[i]);
                    }
                }
            }
            return sb.ToString();
        }
    }
}
