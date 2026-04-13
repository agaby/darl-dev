/// <summary>
/// IDarlMetaRuntime.cs - Core module for the Darl.dev project.
/// </summary>

﻿using Darl.Common;
using DarlCompiler.Parsing;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Darl.Thinkbase.Meta
{
    public interface IDarlMetaRunTime
    {
        bool licensed { get; }

        HashSet<SalienceRecord> CalculateKGSaliences(HashSet<SalienceRecord> saliences, KnowledgeState ks, ParseTree tree);
        Dictionary<string, double> CalculateSaliences(List<DarlResult> currentState, ParseTree tree);
        ParseTree CreateTree(string source, GraphObject node, IGraphModel model);
        ParseTree CreateTreeEdit(string source);
        Task<DarlMetaActivity?> Evaluate(ParseTree parseTree, List<DarlResult> inputs, KnowledgeState ks, FuzzyTime? evalTime = null);
        List<GraphObject> ExploreGraph(ParseTree tree);
        DarlMineReport MineSupervised(PreparedLearningSet ps);
        void SetEvaluationTime(List<DarlTime> now);
        void SetLicense(string license);
        string TermToDarl(DarlMetaNode node);
        string ToDarl(ParseTree parseTree);
    }
}
