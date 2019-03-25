using Darl.GraphQL.Models.Connectivity;
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
        public MLModelType(IConnectivity connectivity)
        {
            Name = "MLModel";
            Description = "A Machine learning model and record data";
            Field(c => c.LastModified);
            Field(c => c.Name);
            Field(c => c.Size);
            Field<MLSpecType>("mlmodel", resolve: context => connectivity.GetMlInternalModelAsync(context.Source.Name));
        }
    }
}
