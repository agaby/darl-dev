using System.Collections.Generic;

namespace Darl.GraphQL.Models.Models
{
    public class QuestionSetInput
    {
        public List<QuestionInput> questions { get; set; }

        public string ieToken { get; set; }

    }
}
