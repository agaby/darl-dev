using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Models
{
    public class MLModel
    {
        public string Name { get; set; }
        public DarlCommon.MLModel MlModel { get; set; }
        public string userId { get; set; }
        public List<MLResult> results { get; set; }

    }
}
