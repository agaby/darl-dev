/// <summary>
/// </summary>

﻿using Darl.Common;
using Darl.Lineage.Bot;
using Darl.Thinkbase.Meta;
using DarlCommon;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static Darl.Lineage.Bot.IBotProcessing;

namespace Darl.Thinkbase
{
    public enum GraphProcess { seek, discover };

    public interface IGraphHandler
    {
        enum SetChoices { three = 3, five = 5, seven = 7, nine = 9 }

        Task<KnowledgeState> Discover(string userId, string KnowledgeGraphName, string subjectId, List<string> lineages, StringBuilder log, FuzzyTime? currentTime);
        Task<(List<InteractTestResponse>, DarlVar?)> GraphPass(KnowledgeState ks, IGraphModel model, string subjectId, string targetId, List<string> paths, string completionLineage, List<DarlVar> values, DarlVar? pending, GraphProcess graphProces, bool debug = false);
        Task<List<InteractTestResponse>> InterpretText(IGraphModel model, string subjectId, DarlVar conversationData);
        Task<KnowledgeState> Seek(KnowledgeState ks, string? targetId, List<string> paths, string completionLineage);
        Task<Meta.DarlMineReport> Learn(string userId, string graphName, string target, LearningForm form, string targetLineage, string valueLineage, int percentTrain = 100, SetChoices sets = SetChoices.three);
        Task<DarlMineReport> Build(string userId, string name, string data, string patternPath, List<DataMap> dataMaps, LoadType ltype = LoadType.xml, LearningForm form = LearningForm.supervised);
    }
}