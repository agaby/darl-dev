using Darl.GraphQL.Models.Connectivity;
using Darl.GraphQL.Models.Middleware;
using Darl.GraphQL.Models.Models;
using Darl.Thinkbase;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class KGraphType : ObjectGraphType<KGraph>
    {
        public KGraphType(IGraphProcessing graph)
        {
            Name = "kGraph";
            Description = "A Knowledge Graph and its status.";
            Field(c => c.Name).Description("The unique name of the knowledge graph");
            Field(c => c.Description).Description("A description of the knowledge graph");
            Field<DateDisplayEnum>("dateDisplay", "Determines if the display form is recent or historic", resolve: context => context.Source.dateDisplay);
            Field<InferenceTimeEnum>("inferenceTime", "Determines if inferences are performed with a current or fixed time.", resolve: context => context.Source.dateDisplay);
            Field<DarlTimeType>("fixedTime", "The time of the inference process if in fixed time mode", resolve: context => context.Source.fixedTime);
            Field<GraphModelType>("model", resolve: context => graph.GetModel(context.Source.userId,context.Source.Name));
            Field<ListGraphType<AuthorizationType>>("authorizations", resolve: context => context.Source.Authorizations).AuthorizeWith("CorpPolicy");
            Field<ServiceConnectivityType>("serviceConnectivity", resolve: context => context.Source.serviceConnectivity).AuthorizeWith("CorpPolicy");
            Field<ListGraphType<UserUsageType>>("usageHistory", resolve: context => context.Source.UsageHistory).AuthorizeWith("CorpPolicy");
        }
    }
}
