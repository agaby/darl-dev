/// </summary>

﻿using Darl.Lineage;
using Darl.Lineage.Bot;
using Darl.Thinkbase.Meta;
using DarlCommon;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Darl.Thinkbase
{
    public interface IMetaStructureHandler
    {
        List<LineageRecord> DefaultNodeLineages { get; }
        List<LineageRecord> DefaultAttLineages { get; }
        List<LineageRecord> DefaultConnLineages { get; }
        ConcurrentDictionary<string, string> CommonLineages { get; }
        Dictionary<string, LineageDefinitionNode> PreloadLineages { get; }

        (DarlVar, InteractTestResponse?) AggregateChildren(GraphObject go, IGraphModel model, string ConnectionLineage);
        bool FindMetaDisplayStructure(IGraphModel model, GraphObject res, ref DarlVar pending, List<InteractTestResponse> responses);
        bool IsConnectionLineage(string lin);
        bool IsObjectLineage(string lin);
        void HandleCodelessValue(IGraphModel model, GraphObject res, DarlVar pending, List<DarlVar> values, KnowledgeState ks);
        void HandleCodelessCompletion(IGraphModel model, GraphObject res, KnowledgeState ks);
        string CreateCompletionRuleSecondPass(IGraphModel model, GraphObject res, List<(string, string, string)> paths, string op);
        List<(string, string)> CreateCompletionRuleFirstPass(IGraphModel model, GraphObject res);
        string GetSuggestedRuleSet(IGraphModel model, string objectId, string lineage);
        bool IsValidLineage(string lin);
        string GetBuildInitialRuleSet(IGraphModel model, string objectId, string target);
        string GetTypeWord(string? lineage);
    }
}
