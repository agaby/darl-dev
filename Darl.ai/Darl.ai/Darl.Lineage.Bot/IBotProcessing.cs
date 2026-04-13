/// <summary>
/// IBotProcessing.cs - Core module for the Darl.dev project.
/// </summary>

﻿using Darl.Thinkbase;
using Darl.Thinkbase.Meta;
using DarlCommon;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Darl.Thinkbase.IGraphHandler;

namespace Darl.Lineage.Bot
{
    public interface IBotProcessing
    {
        public enum LearningForm { supervised, unsupervised, association, unsupervised_cluster }

        Task<List<InteractTestResponse>> InteractKGAsync(string userId, string KnowledgeGraphName, string conversationId, DarlVar conversationData, bool debug = false);
        Task<KnowledgeState> Discover(string userId, string KnowledgeGraphName, string subjectId);
        Task<KnowledgeState> Seek(KnowledgeState ks, string? targetId, List<string> paths, string completionLineage);
        Task<DarlMineReport> Learn(string userId, string graphName, string target, LearningForm form, string targetLineage, string valueLineage, int percentTrain, SetChoices sets);
        Task<DarlMineReport> Build(string userId, string name, string data, string patternPath, List<DataMap> dataMaps, LoadType ltype = LoadType.xml, LearningForm form = LearningForm.supervised);
        IObservable<KnowledgeState> ObservableKStates();
        Task<KnowledgeState?> GetInteractKnowledgeState(string id, string userId, string graphName, bool external = false);

    }
}
