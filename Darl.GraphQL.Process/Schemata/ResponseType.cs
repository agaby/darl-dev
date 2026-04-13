/// <summary>
/// ResponseType.cs - Core module for the Darl.dev project.
/// </summary>

﻿using DarlCommon;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class ResponseDataType : ObjectGraphType<ResponseProxy>
    {
        public ResponseDataType()
        {
            Name = "Response";
            Description = "A response at the completion of a questionnaire";
            Field(c => c.annotation, true).Description("The description of the answer");
            Field(c => c.color, true).Description("The color of any score bar");
            Field(c => c.format, true).Description("The display format of a result");
            Field(c => c.highText, true).Description("High end text of a score bar");
            Field(c => c.lowText, true).Description("Low end text of a score bar");
            Field(c => c.mainText, true).Description("Textual representation of the result");
            Field(c => c.maxVal, true).Description("Numeric max of the score bar");
            Field(c => c.minVal, true).Description("Numeric min of the score bar");
            Field(c => c.preamble, true).Description("Preamble text");
            Field<ResponseTypeEnum>("rType", "The type of the result", resolve: c => c.Source.rtype);
            Field(c => c.value, true).Description("Numeric value if score bar");
        }
    }
}
