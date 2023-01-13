using DarlCommon;
using GraphQL.Types;

namespace Darl.GraphQL.Process.Blazor.Schemata
{
    public class FormFormatType : ObjectGraphType<FormFormat>
    {
        public FormFormatType()
        {
            Name = "FormFormat";
            Description = "Details about the presentation of questionnaires";
            Field(c => c.DefaultQuestions).DefaultValue(1);
            Field<ListGraphType<InputFormatType>>("inputFormatList").Resolve(context => context.Source.InputFormatList);
            Field<ListGraphType<OutputFormatType>>("outputFormatList").Resolve(context => context.Source.OutputFormatList);
        }
    }
}
