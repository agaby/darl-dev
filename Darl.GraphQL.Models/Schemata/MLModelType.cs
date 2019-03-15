using Darl.GraphQL.Models.Models;
using Darl.GraphQL.Models.Services;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class MLModelType : ObjectGraphType<MLModel>
    {
        public MLModelType(IMLModelService mlmodels)
        {
            Field(c => c.LastModified);
            Field(c => c.Name);
            Field<MLModelType>("mlmodel", resolve: context => mlmodels.GetMlModelAsync(context.Source.Name));
        }
    }
}
