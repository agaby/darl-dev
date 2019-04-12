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
            Field<NonNullGraphType<FloatGraphType>>("increment");
            Field<NonNullGraphType<IntGraphType>>("maxLength");
            Field<NonNullGraphType<FloatGraphType>>("numericMax");
            Field<NonNullGraphType<FloatGraphType>>("numericMin");
            Field<NonNullGraphType<StringGraphType>>("regex");
            Field<NonNullGraphType<BooleanGraphType>>("showSets");
            Field<NonNullGraphType<BooleanGraphType>>("enforceCrisp");
            Field<NonNullGraphType<StringGraphType>>("path");
        }
    }
}
