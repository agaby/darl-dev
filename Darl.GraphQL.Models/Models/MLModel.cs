using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Models
{
    public class MLModel
    {
        public MLModel(string name, DarlCommon.MLModel mlModel = null)
        {
            Name = name;
            MlModel = mlModel;
        }

        public string Name { get; }
        public DarlCommon.MLModel MlModel { get; }
        public string userId { get; set; }
        public List<MLResult> results { get; set; }

    }
}
