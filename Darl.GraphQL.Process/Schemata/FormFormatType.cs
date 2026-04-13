/// <summary>
/// </summary>

﻿using DarlCommon;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class FormFormatType : ObjectGraphType<FormFormat>
    {
        public FormFormatType()
        {
            Name = "FormFormat";
            Description = "Details about the presentation of questionnaires";
            Field(c => c.DefaultQuestions).DefaultValue(1);
            Field<ListGraphType<InputFormatType>>("inputFormatList", resolve: context => context.Source.InputFormatList);
            Field<ListGraphType<OutputFormatType>>("outputFormatList", resolve: context => context.Source.OutputFormatList);
        }
    }
}
