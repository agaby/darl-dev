/// </summary>

﻿using GraphQL.Types;
using static Darl.Lineage.Bot.IBotProcessing;

namespace Darl.GraphQL.Models.Schemata
{
    public class LearningFormEnum : EnumerationGraphType<LearningForm>
    {
        public LearningFormEnum()
        {
            Name = "learningForm";
            Description = "Possible forms of learning.";
        }
    }
}
