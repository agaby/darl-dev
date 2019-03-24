using DarlCommon;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public  class InputFormatType : ObjectGraphType<InputFormat>
    {

        public InputFormatType()
        {
            Field(c => c.Name);
            Field<ListGraphType<StringGraphType>>("categories", resolve: context => context.Source.Categories);
            Field(c => c.EnforceCrisp).DefaultValue(false);
            Field(c => c.Increment).DefaultValue(0.0);
            Field<InputTypeEnum>("inputType", resolve: context => context.Source.InType); 
            Field(c => c.MaxLength).DefaultValue(0);
            Field(c => c.NumericMax).DefaultValue(0.0);
            Field(c => c.NumericMin).DefaultValue(0.0);
            Field(c => c.path,true).DefaultValue(string.Empty);
            Field(c => c.Regex, true).DefaultValue(string.Empty); 
            Field(c => c.ShowSets).DefaultValue(false);
        }
    }
}