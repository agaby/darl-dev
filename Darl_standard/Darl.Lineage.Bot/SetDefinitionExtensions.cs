using DarlCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Darl.Lineage.Bot
{
    public static class SetDefinitionExtensions
    {

        public static string ToDarl(this SetDefinition sd)
        {
            var sb = new StringBuilder();
            sb.Append("{" + sd.name + ", ");
            int valCount = 0;
            foreach (double d in sd.values)
            {
                valCount++;
                sb.Append(d.ToString() + (valCount == sd.values.Count ? "" : ","));
            }
            sb.Append("}");
            return sb.ToString();
        }
    }
}
