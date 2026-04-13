/// <summary>
/// IGraphModel.cs - Core module for the Darl.dev project.
/// </summary>

﻿using Darl.Common;
using Darl.Lineage;
using DarlCommon;
using System.Collections.Generic;

namespace Darl.Thinkbase
{
    public interface IGraphModel
    {
        public enum DateDisplay { recent, historic };
        public enum InferenceTime { now, @fixed };

        string modelName { get; set; }
        Dictionary<string, GraphObject> vertices { get; set; }
        Dictionary<string, GraphConnection> edges { get; set; }
        Dictionary<string, GraphObject> virtualVertices { get; set; }
        Dictionary<string, GraphConnection> virtualEdges { get; set; }
        Dictionary<string, GraphObject> recognitionRoots { get; set; }
        Dictionary<string, GraphObject> recognitionVertices { get; set; }
        Dictionary<string, GraphConnection> recognitionEdges { get; set; }
        Dictionary<string, IDynamicConverter> dynamicSources { get; set; }

        List<GraphObject> GetConnectedObjects(GraphObject node, string connectionLineage, string objectLineage);

        (string?,string?) FindControlAttribute(string id);

        DarlVar? FindDataAttribute(string id, string lineage, KnowledgeState ks);
        List<DarlTime?>? FindAttributeExistence(string id, string lineage, KnowledgeState ks);
        GraphAttribute? FindDataGraphAttribute(string id, string lineage, KnowledgeState ks);
        List<LineageRecord> GetLineages(GraphElementType gtype);
        (string, string) SplitCompositeLineage(string comp);
        void SanityCheck();
        void Clear();
        void AddDefaultContent();
        void FollowHypernymy(GraphObject g, List<GraphObject> list);

        bool licensed { get; }
        string key { get; set; }
        string description { get; set; }
        string initialText { get; set; }
        string author { get; set; }
        string copyright { get; set; }
        string licenseUrl { get; set; }
        DateDisplay? dateDisplay { get; set; }
        InferenceTime? inferenceTime { get; set; }
        DarlTime? fixedTime { get; set; }
        bool transient { get; set; }
        string? defaultTarget { get; set; }


    }
}
