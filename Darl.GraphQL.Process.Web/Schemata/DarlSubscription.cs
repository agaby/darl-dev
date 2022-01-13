using Darl.GraphQL.Models.Connectivity;
using Darl.GraphQL.Models.Schemata;
using GraphQL;
using GraphQL.Resolvers;
using GraphQL.Subscription;
using GraphQL.Types;
using System;
using System.Threading.Tasks;

namespace Darl.GraphQL.Web.Models.Schemata
{
    public class DarlSubscription : ObjectGraphType
    {
        private IKGTranslation _trans;

        public DarlSubscription(IKGTranslation trans)
        {
            _trans = trans;
            Name = "Subscription";
            AddField(new EventStreamFieldType()
            { 
                Name = "graphChanged",
                Description = "Respond to KG changes of state",
                Arguments = new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "graphName" }
                ),
                Type = typeof(KnowledgeStateType),
                Resolver = new FuncFieldResolver<KnowledgeStateType>(ResolveObject),
                AsyncSubscriber = new AsyncEventStreamResolver<KnowledgeStateType>(SubscribeGraphChangedAsync)
            }).AuthorizeWith("CorpPolicy");
        }

 
        private Task<IObservable<KnowledgeStateType>> SubscribeGraphChangedAsync(IResolveEventStreamContext arg)
        {
            var graphName = arg.GetArgument<string>("graphName");
            var userId = _trans.GetCurrentUserId(arg.UserContext);

            throw new NotImplementedException();
        }

        private KnowledgeStateType ResolveObject(IResolveFieldContext arg)
        {
            var message = arg.Source as KnowledgeStateType;
            return message;
        }
    }
}