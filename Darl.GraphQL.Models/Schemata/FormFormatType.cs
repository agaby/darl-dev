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
        public FormFormatType()
        {
            Field(c => c.DefaultQuestions).DefaultValue(1); 
            Field<ListGraphType<InputFormatType>>("inputFormatList", resolve: context => context.Source.InputFormatList);
            Field<ListGraphType<InputFormatType>>("outputFormatList", resolve: context => context.Source.OutputFormatList);
        }
    }
}
