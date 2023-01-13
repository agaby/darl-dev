using GraphQL.Types;

namespace Darl.GraphQL.Process.Blazor.Schemata
{
    public class MLSpecType : ObjectGraphType<DarlCommon.MLModel>
    {
        public MLSpecType()
        {
            Name = "MLSpec";
            Description = "Machine learning model detail";
            Field(c => c.author, true);
            Field(c => c.copyright, true);
            Field(c => c.darl);
            Field(c => c.dataSchema, true);
            Field(c => c.description, true);
            Field(c => c.destinationRulesetName, true);
            Field(c => c.license, true);
            Field(c => c.name);
            Field(c => c.percentTest).DefaultValue(0);
            Field(c => c.sets).DefaultValue(3);
            Field(c => c.trainData, true);
            Field(c => c.version, true);
        }
    }
}
