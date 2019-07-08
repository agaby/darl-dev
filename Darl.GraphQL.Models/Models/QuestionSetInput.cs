using DarlCommon;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Models
{
    public class QuestionSetInput
    {
        public List<QuestionInput> questions { get; set; }

        public string ieToken { get; set; }

    }
}
