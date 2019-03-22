using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Models
{
    public class MLModel
    {
        public MLModel(DateTime lastModified, string name, int size, DarlCommon.MLModel mlModel = null)
        {
            LastModified = lastModified;
            Name = name;
            MlModel = mlModel;
        }

        public DateTime LastModified { get; }
        public string Name { get; }
        public DarlCommon.MLModel MlModel { get; }
    }
}
