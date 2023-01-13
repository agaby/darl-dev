using DarlCommon;

namespace Darl.GraphQL.Process.Blazor.Models
{
    public class QuestionInput
    {
        public string reference { get; set; }
        public string sResponse { get; set; }
        public double dResponse { get; set; }
        public QuestionProxy.QType qType { get; set; }
    }
}
