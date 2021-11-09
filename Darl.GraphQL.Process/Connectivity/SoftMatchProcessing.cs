using Darl.SoftMatch;
using Darl.Thinkbase;
using GraphQL;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Connectivity
{
    public class SoftMatchProcessing : ISoftMatchProcessing
    {
        private readonly IBlobConnectivity _blob;
        private readonly ILogger _logger;
        public SoftMatchProcessing(IBlobConnectivity blob, ILogger<SoftMatchProcessing> logger)
        {
            _blob = blob;
            _logger = logger;
        }

        public async Task<string> CreateSoftMatchModel(string userId, string treeName, List<StringStringPair> data, bool rebuild = false)
        {
            MatchList graph;
            var blobName = GenerateGraphName(userId, treeName);
            if (!rebuild && await _blob.Exists(blobName))
            {
                graph = MatchList.DeserializeGraph(await _blob.Read(blobName));
            }
            else
            {
                graph = new MatchList();
            }
            var intLabels = new List<KeyValuePair<string, string>>();
            foreach (var l in data)
            {
                intLabels.Add(new KeyValuePair<string, string>(l.Name, l.Value));
            }
            graph.CreateTree(intLabels);
            await _blob.Write(blobName, graph.SerializeGraph());
            return $"Match Model {treeName} created containing {data.Count()} texts.";
        }

        public static string GenerateGraphName(string userId, string treeName)
        {
            return userId + "_" + treeName.Replace(" ", "_");
        }



        public async Task<List<MatchResult>> InferFromSoftMatchModel(string userId, string treeName, List<string> texts)
        {
            var blobName = GenerateGraphName(userId, treeName);

            if (!await _blob.Exists(blobName))
                throw new ExecutionError($"Concept match tree {treeName} not found in this account.");
            var graph = MatchList.DeserializeGraph(await _blob.Read(blobName));
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

        public Task<List<string>> ListSoftMatchModels(string userId)
        {
            return Task.FromResult(_blob.List(userId));
        }

        public async Task<string> DeleteSoftMatchModel(string userId, string name)
        {
            if (await _blob.Delete(GenerateGraphName(userId, name)))
                return "Deleted";
            else
                return $"{name} not found";
        }
    }
}
