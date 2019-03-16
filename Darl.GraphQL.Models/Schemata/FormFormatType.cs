using Darl.GraphQL.Models.Services;
using DarlCommon;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class FormFormatType : ObjectGraphType<FormFormat>
    {
        public FormFormatType(IInputFormatService inputformats, IOutputFormatService outputformats)
        {
            Field(c => c.DefaultQuestions);
            Field(c => c.Edited);
            Field<ListGraphType<InputFormatType>>("inputFormatList", resolve: context => inputformats.GetInputFormatsAsync());
            Field<ListGraphType<InputFormatType>>("outputFormatList", resolve: context => outputformats.GetOutputFormatsAsync());
        }
    }
}
