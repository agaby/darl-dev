using GraphQL.Types;
using static Darl.Thinkbase.IGraphHandler;

namespace Darl.GraphQL.Models.Schemata
{
    public class SetChoiceEnum : EnumerationGraphType<SetChoices>
    {
        public SetChoiceEnum()
        {
            Name = "setChoiceEnum";
            Description = "possible numbers of fuzzy sets to use for numeric modelling in Machine learning. ";

        }
    }
}

