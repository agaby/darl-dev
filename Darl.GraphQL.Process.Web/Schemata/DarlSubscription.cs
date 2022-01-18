using Darl.GraphQL.Models.Connectivity;
using Darl.GraphQL.Models.Models;
using Darl.GraphQL.Models.Schemata;
using Darl.Lineage.Bot;
using Darl.Thinkbase;
using GraphQL;
using GraphQL.Resolvers;
using GraphQL.Subscription;
using GraphQL.Types;
using System;
using System.Threading.Tasks;
using System.Reactive.Linq;

namespace Darl.GraphQL.Web.Models.Schemata
{
    public class DarlSubscription : ObjectGraphType
    {
        private IKGTranslation _trans;
        private IBotProcessing _bot;
        private IGraphProcessing _graph;

        public DarlSubscription(IKGTranslation trans, IBotProcessing bot, IGraphProcessing graph)
        {
            _trans = trans;
            _bot = bot;
            _graph = graph;

            Name = "Subscription";
            AddField(new EventStreamFieldType()
            { 
                Name = "graphChanged",
                Description = "Respond to KG changes of state",
                Arguments = new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "graphName", Description= "The Knowledge graph to infer from"},
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "target", Description = "The object to seek or discover from" },
                    new QueryArgument<ThinkBaseProcessType> { Name = "process", Description = "Whether to seek the target or discover paths from the target", DefaultValue = GraphProcess.seek }
                ),
                Type = typeof(KnowledgeStateType),
                Resolver = new FuncFieldResolver<KnowledgeState>(ResolveObject),
                AsyncSubscriber = new AsyncEventStreamResolver<KnowledgeState>(SubscribeGraphChangedAsync)
            }).AuthorizeWith("CorpPolicy");
        }

 
        private async Task<IObservable<KnowledgeState>> SubscribeGraphChangedAsync(IResolveEventStreamContext arg)
        {

            var userId = _trans.GetCurrentUserId(arg.UserContext);
            var graphName = arg.GetArgument<string>("graphName");
            var process = arg.GetArgument<GraphProcess>("process");
            var target = arg.GetArgument<string>("target");
            
            var ks = _graph.ObservableKStates();
            return ks.Where(a => a.userId == userId && a.knowledgeGraphName == graphName).Process( _bot, process, target);
        }

        private KnowledgeState ResolveObject(IResolveFieldContext arg)
        {
            var state = arg.Source as KnowledgeState;
            return state;
        }
    }
}