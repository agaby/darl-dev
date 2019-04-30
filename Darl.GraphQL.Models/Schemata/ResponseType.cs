using DarlCommon;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class ResponseType : ObjectGraphType<ResponseProxy>
    {
        public ResponseType()
        {
            Name = "ResponseType";
            Description = "A response at the completion of a questionnaire";
            Field(c => c.annotation);
            Field(c => c.color);
            Field(c => c.format);
            Field(c => c.highText);
            Field(c => c.lowText);
            Field(c => c.mainText);
            Field(c => c.maxVal);
            Field(c => c.minVal);
            Field(c => c.preamble);
            Field<ResponseTypeEnum>("responseType", resolve: c => c.Source.rtype);//
            Field(c => c.value);
        }
    }
}
