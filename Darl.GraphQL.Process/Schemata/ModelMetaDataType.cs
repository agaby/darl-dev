/// <summary>
/// </summary>

﻿using Darl.Thinkbase;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class ModelMetaDataType : ObjectGraphType<ModelMetaData>
    {

        public ModelMetaDataType()
        {
            Name = "modelMetaData";
            Description = "Meta data for a knowledge graph";
            Field(c => c.author, true);
            Field(c => c.copyright, true);
            Field<DateDisplayEnum>("dateDisplay", resolve: c => c.Source.dateDisplay);
            Field(c => c.description, true);
            Field<DarlTimeType>("fixedTime", resolve: c => c.Source.fixedTime);
            Field<InferenceTimeEnum>("inferenceTime", resolve: c => c.Source.inferenceTime);
            Field(c => c.initialText, true);
            Field(c => c.licenseUrl, true);
            Field(c => c.defaultTarget, true);
        }
    }
}
