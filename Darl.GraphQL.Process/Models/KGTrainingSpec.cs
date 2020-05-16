using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Models
{
    public class KGTrainingSpec
    {
        public List<KGTrainingValue> values { get; set; } = new List<KGTrainingValue>();
    }
}
