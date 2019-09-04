using Darl.GraphQL.Models.Models;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class InputFormatUpdateType : InputObjectGraphType<InputFormatUpdate>
    {

        public InputFormatUpdateType()
        {
            Name = "inputFormatInput";
            Description = "Format for an input used in a questionnaire";
            Field<FloatGraphType>("increment");
            Field<IntGraphType>("maxLength");
            Field<FloatGraphType>("numericMax");
            Field<FloatGraphType>("numericMin");
            Field<StringGraphType>("regex");
            Field<BooleanGraphType>("showSets");
            Field<BooleanGraphType>("enforceCrisp");
            Field<StringGraphType>("path");
        }
    }
}
