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
        public MLModelType()
        {
            Field(c => c.LastModified);
            Field(c => c.Name);
            Field<MLSpecType>("mlmodel", resolve: context => context.Source.MlModel);
        }
    }
}
