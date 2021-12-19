using Darl.Lineage.Bot;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class BotTestViewType : ObjectGraphType<BotTestView>
    {
        public BotTestViewType()
        {
            Name = "botTestView";
            Description = "Holds values of internal states for bot testing";
            Field(c => c.conversationID).Description("Identifies the conversation");
            Field(c => c.darl).Description("The generated code");
            Field<ListGraphType<StoreStateType>>("stores", "The store data", resolve: context => context.Source.stores);
            Field<ListGraphType<StringGraphType>>("conversation", "Conversation history", resolve: context => context.Source.conversation);
            Field(c => c.userText, true).Description("The user text");
        }
    }
}
