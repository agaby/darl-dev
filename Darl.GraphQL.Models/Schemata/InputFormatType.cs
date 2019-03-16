using DarlCommon;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    internal class InputFormatType : ObjectGraphType<InputFormat>
    {

        public InputFormatType()
        {
            Field(c => c.Name);
            Field(c => c.Categories);
            Field(c => c.EnforceCrisp);
            Field(c => c.Increment);
            Field<InputTypeEnum>("inputType", resolve: context => context.Source.InType); 
            Field(c => c.MaxLength);
            Field(c => c.NumericMax);
            Field(c => c.NumericMin);
            Field(c => c.path);
            Field(c => c.Regex);
            Field(c => c.ShowSets);
        }
    }
}