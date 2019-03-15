using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Models
{
    public class MLModel
    {
        public MLModel(DateTime lastModified, string name, int size, MLModel mlModel)
        {
            LastModified = lastModified;
            Name = name;
            MlModel = mlModel;
        }

        public DateTime LastModified { get; }
        public string Name { get; }
        public MLModel MlModel { get; }
    }
}
