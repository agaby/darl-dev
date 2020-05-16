using Darl.GraphQL.Models.Models;
using Darl.Lineage;
using GraphQL;
using Gremlin.Net.Structure;
using Microsoft.Extensions.Logging;
using QuickGraph.Algorithms.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Connectivity
{
    public class ConceptMatchProcessing : IConceptMapProcessing
    {
        private IBlobConnectivity _blob;
        private ILogger _logger;
        public ConceptMatchProcessing(IBlobConnectivity blob, ILogger<ConceptMatchProcessing> logger)
        {
            _blob = blob;
            _logger = logger;
        }

        public async Task<string> CreateConceptMatchTree(string userId, string treeName, List<StringStringPair> data, bool rebuild = false)
        {
            MatchGraph graph;
            var blobName = GenerateGraphName(userId, treeName);
            if (!rebuild && await _blob.Exists(blobName))
            {
                graph = MatchGraph.DeserializeGraph(await _blob.Read(blobName));
            }
            else
            {
                graph = new MatchGraph();
            }
            graph.CreateTree(data);
            await _blob.Write(blobName, graph.SerializeGraph());
            return $"Match Model {treeName} created containing {data.Count()} texts.";
        }

        private string GenerateGraphName(string userId, string treeName)
        {
            return userId + "_" + treeName.Replace(" ", "_");
        }


 
        public async Task<List<MatchResult>> InferFromConceptMatchTree(string userId, string treeName, List<string> texts)
        {
            var blobName = GenerateGraphName(userId, treeName);

            if(!await _blob.Exists(blobName))
                throw new ExecutionError($"Concept match tree {treeName} not found in this account.");
            var graph = MatchGraph.DeserializeGraph(await _blob.Read(blobName));
            var responses = new List<MatchResult>(texts.Count);
            int index = 0;
            foreach (var text in texts)
            {
                responses.Add(graph.Find(text));
                graph.Flush();
                index++;
            }
            return responses;
        }
    }
}
