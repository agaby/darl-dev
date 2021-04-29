using Darl.GraphQL.Models.Models;
using Darl.Thinkbase;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Darl.GraphQL.Process.Connectivity
{
    /// <summary>
    /// Transforms between GraphQL processes and KGs
    /// </summary>
    public class KGTranslation : IKGTranslation
    {
        private IConfiguration _config;
        private ILogger<KGTranslation> _logger;
        private IGraphProcessing _graph;
        private static string updateKG = "updates.graph";
        private static string sourceLineage = "noun:01,4,04,02,21,16";
        private static string destinationLineage = "noun:01,0,0,15,15,3";
        private static string processLineage = "noun:00,4";
        public KGTranslation(ILogger<KGTranslation> logger, IConfiguration config, IGraphProcessing graph)
        {
            _config = config;
            _logger = logger;
            _graph = graph;
            updateKG = _config["AppSettings:boaiuserid"] + '_' + updateKG;
        }


        public async Task<List<Update>> Updates()
        {
            var list = new List<Update>();
            var obs = await _graph.GetGraphObjectsByLineage(updateKG, processLineage);
            foreach(var o in obs)
            {
                var from = o.properties.FirstOrDefault(a => a.lineage.StartsWith(sourceLineage));
                var to = o.properties.FirstOrDefault(a => a.lineage.StartsWith(destinationLineage));
                if(from != null && to != null)
                {
                    var update = new Update { from = from.value, to = to.value, updated = o.existence.Last() != null ? o.existence.Last().dateTime : DateTime.MinValue };
                    list.Add(update);
                }
            }
            return list;
        }

        public async Task<DateTime> GetLastUpdate(string from, string to)
        {
            var obj = await GetUpdate(from, to);
            if(obj != null)
            {
                return obj.existence.Last() != null ? obj.existence.Last().dateTime : DateTime.MinValue;
            }
            return DateTime.MinValue;
        }

        public DateTime SetLastUpdate(string from, string to)
        {
            return DateTime.Now;
        }

        private async Task<GraphObject?> GetUpdate(string from, string to)
        {
            var obs = await _graph.GetGraphObjectsByLineage(updateKG, processLineage);
            foreach (var o in obs)
            {
                var tfrom = o.properties.FirstOrDefault(a => a.lineage.StartsWith(sourceLineage));
                var tto = o.properties.FirstOrDefault(a => a.lineage.StartsWith(destinationLineage));
                if (tfrom != null && tto != null)
                {
                    if (tfrom.value == from && tto.value == to)
                        return o;
                }
            }
            return null;
        }
    }
}
