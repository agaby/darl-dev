using Darl.Common;
using Darl.GraphQL.Process.Blazor.Models;
using Darl.Licensing;
using Darl.Lineage;
using Darl.Thinkbase;
using Darl.Thinkbase.Meta;
using DarlCommon;
using DarlCompiler;
using GraphQL;
//using GraphQL.Server.Transports.Subscriptions.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using ThinkBase.ComponentLibrary.Models;

namespace Darl.GraphQL.Process.Blazor.Connectivity
{
    /// <summary>
    /// Transforms between GraphQL processes and KGs
    /// </summary>
    public class KGTranslation : IKGTranslation
    {
        private readonly IConfiguration _config;
        private readonly ILogger<KGTranslation> _logger;
        private readonly IGraphProcessing _graph;
        private readonly IMetaStructureHandler _meta;
        private readonly IMemoryCache _localCache;
        private static readonly string existenceLineage = "noun:01,5,03,3,018";//life
        static readonly string objectIdClaimText = @"http://schemas.microsoft.com/identity/claims/objectidentifier";
        static readonly string tenantIdClaimText = @"http://schemas.microsoft.com/identity/claims/tenantid";




        public KGTranslation(ILogger<KGTranslation> logger, IConfiguration config, IGraphProcessing graph, IMetaStructureHandler meta, ILicensing licensing, IMemoryCache localCache)
        {
            _config = config;
            _logger = logger;
            _graph = graph;
            _meta = meta;
            _localCache = localCache;
        }


        public async Task<bool> CreateNewGraph(string userId, string modelName)
        {
            return await _graph.CreateNewGraph(userId, modelName);
        }

        public async Task<string> GetSuggestedRuleSet(string userId, string modelName, string objectId, string lineage)
        {
            var model = await _graph.GetModel(userId, modelName);
            if (model != null)
            {
                return _meta.GetSuggestedRuleSet(model, objectId, lineage);
            }
            return string.Empty;
        }


        public Task<string> GetTypeWordForLineage(string lineage, string isoLanguage = "en")
        {
            try
            {
                if (LineageLibrary.lineages.ContainsKey(lineage))
                    return Task.FromResult(LineageLibrary.lineages[lineage].typeWord);
                return Task.FromResult(string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogDebug($"Bad lineage lookup for lineage {lineage} message: {ex.Message}");
                return Task.FromResult(string.Empty);
            }
        }

        public Task<List<LineageRecord>> GetLineagesForWord(string word, string isoLanguage = "en")
        {
            try
            {
                var offset = 0;
                return Task.FromResult(LineageLibrary.WordRecognizer(new List<string> { word }, ref offset, true));
            }
            catch (Exception ex)
            {
                _logger.LogDebug($"Bad lineage lookup for word {word} message: {ex.Message}");
                return Task.FromResult(new List<LineageRecord>());
            }
        }

        public Task<List<DarlLintView>> LintDarlMeta(string darl)
        {
            var errorList = new List<DarlLintView>();
            int rowoffset = 0;
            int coloffset = 0;
            if (!string.IsNullOrEmpty(darl))
            {
                try
                {
                    var metaRuntime = new DarlMetaRunTime(_config, _meta);
                    var tree = metaRuntime.CreateTreeEdit(darl);
                    if (tree.HasErrors())
                    {
                        foreach (var pm in tree.ParserMessages)
                        {
                            errorList.Add(new DarlLintView { line_no = pm.Location.Line + 1 - rowoffset, column_no_start = pm.Location.Column + 1 - coloffset, column_no_stop = pm.Location.Column + 2 - coloffset, message = pm.Message, severity = pm.Level == ErrorLevel.Error ? "error" : "warning" });
                        }
                    }
                }
                catch (Exception)
                {

                }

            }
            return Task.FromResult(errorList);
        }

        public async Task<List<GraphAttribute>> GetConceptCloudData(string userId, string graphName, string address)
        {
            var list = new List<GraphAttribute>();
            if (!await _graph.Exists(userId, graphName))
                throw new ExecutionError($"{graphName} does not exist in this account");
            var graph = await _graph.GetModel(userId, graphName);
            if (string.IsNullOrEmpty(address))//root
            {//return the type words for the real objects derived from the virtual
                foreach (var g in graph.virtualVertices.Values.Where(a => a.In.Count == 0)) //All leaf virtual vertices
                {
                    list.Add(new GraphAttribute { value = (g.name ?? "").Replace('/', '~'), name = "typeword", type = GraphAttribute.DataType.textual, lineage = g.lineage });
                }
            }
            else
            {
                var parts = address.Split('/');
                int depth = 0;
                GraphObject? root = null;
                GraphObject? real = null;
                GraphAttribute att;
                foreach (var part in parts)
                {
                    if (!string.IsNullOrEmpty(part))
                    {
                        var name = part.Trim();
                        name = name.Replace('~', '/');
                        switch (depth)
                        {
                            case 0: //leaf lineages
                                root = graph!.virtualVertices.Values.FirstOrDefault(a => a.name == name);
                                break;
                            case 1://real nodes
                                real = graph!.vertices.Values.FirstOrDefault(a => a.externalId == name);
                                break;
                            case 2://attributes
                                att = real!.properties!.FirstOrDefault(a => a.name == name);
                                break;
                        }
                        depth++;
                    }
                }
                switch (depth)
                {
                    case 1: //only a virtual node selected
                        {
                            var children = graph!.vertices.Values.Where(a => a.lineage == root!.lineage);
                            if (children.Count() > 1)
                            {
                                foreach (var g in children)
                                {
                                    list.Add(new GraphAttribute { value = (g.name ?? "").Replace('/', '~'), name = "typeword", type = GraphAttribute.DataType.textual, lineage = g.lineage });
                                }
                            }
                            else
                            {
                                real = children.FirstOrDefault();
                                if (real != null)
                                    list.AddRange(real.properties!);
                            }
                        }
                        break;
                    case 2:
                        list.AddRange(real!.properties!);
                        break;
                }
            }
            return list;
        }




        public async Task<string> CreateTempKG(string userId, string graphName, IFormFile file)
        {
            using (var fs = file.OpenReadStream())
            using (var ws = new MemoryStream())
            {
                await fs.CopyToAsync(ws);
                ws.Position = 0;
                return await _graph.CreateTempKG(userId, graphName, ws.ToArray());
            }
        }

        public async Task<bool> TempKGExists(string userId, string graphName)
        {
            return await _graph.ExistsInCache(userId, graphName);
        }

        public async Task<byte[]> KGContents(string userId, string graphName)
        {
            return await _graph.KGContents(userId, graphName);
        }


        public async Task Promote(string userId, string tenantId, string name)
        {
        }

        #region private

        private string CompositeName(string userId, string graphName)
        {
            return $"{userId}_{graphName}";
        }



        private string GetAttributeValue(KnowledgeState ks, string objectId, string lineage)
        {
            var att = ks.GetAttribute(objectId, lineage);
            if (att != null)
            {
                return att.value ?? string.Empty;
            }
            return string.Empty;
        }

        private List<DarlTime?> GetExistence(KnowledgeState ks, string objectId)
        {
            var att = ks.GetAttribute(objectId, existenceLineage);
            if (att != null)
            {
                return att.existence;
            }
            return new List<DarlTime?>();
        }

        private DateTime GetExistenceStart(KnowledgeState ks, string objectId)
        {
            var att = ks.GetAttribute(objectId, existenceLineage);
            if (att != null)
            {
                if (att.existence != null)
                {
                    var first = att.existence.First();
                    if (first != null)
                    {
                        return first.dateTime;
                    }
                }
            }
            return DateTime.MinValue;
        }



        private void UpdateAttribute(KnowledgeState ks, string objectId, string attlineage, string name, string value, bool overwrite = false)
        {
            if (string.IsNullOrEmpty(value))
                return;
            if (!ks.data.ContainsKey(objectId))
                return;
            var properties = ks.data[objectId];
            var val = properties.FirstOrDefault(a => a.lineage.StartsWith(attlineage));
            if (val == null)
            {
                properties.Add(new GraphAttribute
                {
                    id = Guid.NewGuid().ToString(),
                    name = name,
                    lineage = attlineage,
                    type = GraphAttribute.DataType.textual,
                    value = value,
                    confidence = 1.0
                });
            }
            else if (string.IsNullOrEmpty(val.value) || overwrite)
            {
                val.value = value;
            }
        }

        private void UpdateExistence(KnowledgeState ks, string objectId, List<DarlTime?> existence)
        {
            if (existence == null || !existence.Any())
                return;
            if (!ks.data.ContainsKey(objectId))
                return;
            var properties = ks.data[objectId];
            var val = properties.FirstOrDefault(a => a.lineage!.StartsWith(existenceLineage));
            if (val == null)
            {
                properties.Add(new GraphAttribute
                {
                    id = Guid.NewGuid().ToString(),
                    name = "existence",
                    lineage = existenceLineage,
                    type = GraphAttribute.DataType.temporal,
                    existence = existence,
                    confidence = 1.0
                });
            }
            else
            {
                val.existence = existence;
            }
        }

        public string GetCurrentUserId(GraphQLUserContext? userContext)
        {
            if (userContext!.User!.Identity!.IsAuthenticated)
            {
                return userContext!.User!.Claims.Where(ai => ai.Type == objectIdClaimText).Single().Value;
            }
            return string.Empty;
        }

        public string GetCurrentTenantId(GraphQLUserContext? userContext)
        {
            if (userContext!.User!.Identity!.IsAuthenticated)
            {
                return userContext!.User!.Claims.Where(ai => ai.Type == tenantIdClaimText).Single().Value;
            }
            return string.Empty;
        }



        public async Task<List<KGraphListElement>> GetKGraphs(string userId, string tenantId)
        {
            var kGraphs =  new List<KGraphListElement>();
            //combine team and individual 
            await GetGraphKind(userId, kGraphs, KGSource.individual);
            await GetGraphKind(tenantId, kGraphs, KGSource.team);
            return kGraphs;
        }

        private async Task GetGraphKind(string id, List<KGraphListElement> kGraphs, KGSource stype)
        {
            var inds = await _graph.GetGraphs(id);
            foreach (var ind in inds)
            {
                var name = ind.Remove(0, id.Length);
                name = name.Replace('/', ' ');
                if (name.Trim().Any())
                {
                    kGraphs.Add(new KGraphListElement { name = name.Trim(), kgSource = stype });
                }
            }
        }

        #endregion
    }
}
