using Darl.GraphQL.Models.Models;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class MLSpecUpdateType : InputObjectGraphType<MLSpecUpdate>
    {

        public MLSpecUpdateType()
        {
            Name = "MLSpecUpdate";
            Field<StringGraphType>("darl");
            Field<StringGraphType>("version");
            Field<StringGraphType>("author");
            Field<StringGraphType>("copyright");
            Field<StringGraphType>("license");
            Field<StringGraphType>("description");
            Field<StringGraphType>("trainData");
            Field<StringGraphType>("dataSchema");
            Field<SetGraphType>("sets");
            Field<PercentGraphType>("percentTest");
            Field<StringGraphType>("destinationRulesetName");
        }
    }
}
