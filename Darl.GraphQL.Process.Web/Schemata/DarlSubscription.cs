using Darl.GraphQL.Models.Connectivity;
using Darl.GraphQL.Models.Schemata;
using Darl.Lineage.Bot;
using Darl.Thinkbase;
using DarlLanguage.Processing;
using GraphQL;
using GraphQL.Resolvers;
using GraphQL.Subscription;
using GraphQL.Types;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using static Darl.Lineage.Bot.IBotProcessing;
using static Darl.Thinkbase.IGraphHandler;

namespace Darl.GraphQL.Web.Models.Schemata
{
    public class DarlSubscription : ObjectGraphType
    {
        private IKGTranslation _trans;
        private IBotProcessing _bot;
        private IGraphProcessing _graph;
        private IConfiguration _config;
        private ILogger _logger;

        private readonly ISubject<Thinkbase.Meta.DarlMineReport> _darlMineReportStream = new ReplaySubject<Thinkbase.Meta.DarlMineReport>(1);

        private readonly ISubject<Thinkbase.Meta.DarlMineReport> _darlMineBuildStream = new ReplaySubject<Thinkbase.Meta.DarlMineReport>(1);


        public DarlSubscription(IKGTranslation trans, IBotProcessing bot, IGraphProcessing graph, IConfiguration config, ILogger<DarlSubscription> logger)
        {
            _trans = trans;
            _bot = bot;
            _graph = graph;
            _config = config;
            _logger = logger;
            Name = "Subscription";
            AddField(new EventStreamFieldType()
            {
                Name = "graphChanged",
                Description = "Respond to KG changes of state",
                Arguments = new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "graphName", Description = "The Knowledge Graph to infer from" },
                    new QueryArgument<StringGraphType> { Name = "target", Description = "The object to seek or discover from if not the default defined in the KG" },
                    new QueryArgument<ThinkBaseProcessType> { Name = "process", Description = "Whether to seek the target or discover paths from the target", DefaultValue = GraphProcess.seek }
                ),
                Type = typeof(KnowledgeStateType),
                Resolver = new FuncFieldResolver<KnowledgeState?>(ResolveKSObject),
                Subscriber = new EventStreamResolver<KnowledgeState>(SubscribeGraphChanged)
            });
            AddField(new EventStreamFieldType()
            {
                Name = "learn",
                Description = "Learn relationships in an existing Knowledge Graph and respond when complete.",
                Arguments = new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "graphName", Description = "The Knowledge Graph to infer from" },
                    new QueryArgument<StringGraphType> { Name = "target", Description = "The object to predict or categorize if not the default defined in the KG" },
                    new QueryArgument<LearningFormEnum> { Name = "learningForm", Description = "The kind of learning to perform.", DefaultValue = LearningForm.supervised },
                    new QueryArgument<StringGraphType> { Name = "targetLineage", Description = "The lineage of the attribute containing the ruleset.", DefaultValue = "adjective:5500" },
                    new QueryArgument<StringGraphType> { Name = "valueLineage", Description = "The lineage of the attribute to predict.", DefaultValue = "noun:01,4,05,21,19" },
                    new QueryArgument<IntGraphType> { Name = "percentTrain", Description = "The percentage of the data to dedicate to training. (The rest will be test data).", DefaultValue = 100 },
                    new QueryArgument<SetChoiceEnum> { Name = "sets", Description = "The number of fuzzy sets to use in numeric modelling.", DefaultValue = SetChoices.three }
                ),
                Type = typeof(DarlMineReportType),
                Resolver = new FuncFieldResolver<Thinkbase.Meta.DarlMineReport?>(ResolveDMRObject),
                Subscriber = new EventStreamResolver<Thinkbase.Meta.DarlMineReport>(SubscribeLearn)
            });
            AddField(new EventStreamFieldType()
            {
                Name = "build",
                Description = "Load training data, build a knowledge graph and learn the relationships in the data.",
                Arguments = new QueryArguments(
                 new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name", Description = "The name of the Knowledge graph to create KStates for" },
                 new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "data", Description = "The XML or Json source" },
                 new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "patternPath", Description = "The XPath or JPath pattern locator" },
                 new QueryArgument<NonNullGraphType<ListGraphType<DataMapType>>> { Name = "dataMaps", Description = "List of maps for individual data items" }
                ),
                Type = typeof(DarlMineReportType),
                Resolver = new FuncFieldResolver<Thinkbase.Meta.DarlMineReport?>(ResolveDMRObject),
                Subscriber = new EventStreamResolver<Thinkbase.Meta.DarlMineReport>(SubscribeBuild)
            });
        }


        private IObservable<KnowledgeState> SubscribeGraphChanged(IResolveEventStreamContext arg)
        {

            var userId = _trans.GetCurrentUserId(arg.UserContext);
            var graphName = arg.GetArgument<string>("graphName");
            var process = arg.GetArgument<GraphProcess>("process");
            var target = arg.GetArgument<string>("target");
            if (userId == _config["AppSettings:boaiuserid"])
            {
                throw new ExecutionError($"Subscriptions only permitted to registered users.");
            }
            var ks = _graph.ObservableKStates();
            return ks.Where(a => a.userId == userId && a.knowledgeGraphName == graphName).Select(i =>  _bot.Seek(i, target, new List<string>(), "adjective:5500").Result);
        }

        private KnowledgeState? ResolveKSObject(IResolveFieldContext arg)
        {
            var state = arg.Source as KnowledgeState;
            _logger.LogInformation($"Returning a Knowledge state: {state}.");
            return state;
        }

        private Thinkbase.Meta.DarlMineReport? ResolveDMRObject(IResolveFieldContext arg)
        {
            var state = arg.Source as Thinkbase.Meta.DarlMineReport;
            _logger.LogInformation($"Returning a Darl Mine Report: {state}.");
            return state;
        }

        /// <summary>
        /// One shot learning. The subscription triggers learning and closes on completion.
        /// </summary>
        /// <param name="arg">The arguments</param>
        /// <returns>The report</returns>
        /// <exception cref="ExecutionError"></exception>
        private IObservable<Thinkbase.Meta.DarlMineReport> SubscribeLearn(IResolveEventStreamContext arg)
        {
            var userId = _trans.GetCurrentUserId(arg.UserContext);
            var graphName = arg.GetArgument<string>("graphName");
            var target = arg.GetArgument<string>("target");
            var form = arg.GetArgument<LearningForm>("learningForm");
            var targetLineage = arg.GetArgument<string>("targetLineage");
            var valueLineage = arg.GetArgument<string>("valueLineage");
            var percentTrain = arg.GetArgument<int>("percentTrain");
            var sets = arg.GetArgument<SetChoices>("sets");
            if (userId == _config["AppSettings:boaiuserid"])
            {
                throw new ExecutionError($"Subscriptions only permitted to registered users.");
            }
            var res = _darlMineReportStream.AsObservable();
            _darlMineReportStream.Subscribe(s =>
            {
                try
                {
                    var dmr = _bot.Learn(userId, graphName, target, form, targetLineage, valueLineage, percentTrain, sets).Result;
                    _darlMineReportStream.OnNext(dmr);
                    _darlMineReportStream.OnCompleted();
                }
                catch (Exception ex)
                {
                    _darlMineReportStream.OnError(ex);
                    _darlMineReportStream.OnCompleted();
                    _logger.LogError(ex, "Exception reported in SubscribeLearn.");
                }
             });
            _darlMineReportStream.OnNext(new Thinkbase.Meta.DarlMineReport());
            return res;
        }

        private IObservable<Thinkbase.Meta.DarlMineReport> SubscribeBuild(IResolveEventStreamContext arg)
        {
            var name = arg.GetArgument<string>("name");
            var data = arg.GetArgument<string>("data");
            var patternPath = arg.GetArgument<string>("patternPath");
            var dataMaps = arg.GetArgument<List<Thinkbase.DataMap>>("dataMaps");
            var userId = _trans.GetCurrentUserId(arg.UserContext);
            if (userId == _config["AppSettings:boaiuserid"])
            {
                throw new ExecutionError($"Subscriptions only permitted to registered users.");
            }
            var res = _darlMineBuildStream.AsObservable();
            _darlMineBuildStream.Subscribe(async s =>
            {
                try
                {
                    var dmr = await _bot.Build(userId, name, data, patternPath, dataMaps);
                    _darlMineBuildStream.OnNext(dmr);
                    _darlMineBuildStream.OnCompleted();
                }
                catch (Exception ex)
                {
                    _darlMineBuildStream.OnError(ex);
                    _darlMineBuildStream.OnCompleted();
                    _logger.LogError(ex, "Exception reported in SubscribeBuild.");
                }
            });
            _darlMineBuildStream.OnNext(new Thinkbase.Meta.DarlMineReport());
            return res;
        }
    }
}