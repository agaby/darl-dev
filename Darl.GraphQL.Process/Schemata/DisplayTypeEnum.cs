using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class DisplayTypeEnum : EnumerationGraphType
    {

        public DisplayTypeEnum()
        {
            Name = "displayTypes";
            Description = "The ways in which a result can be presented.";
            AddValue("TEXT", "display as text", 1);
            AddValue("SCOREBAR", "display as a score bar", 2);
            AddValue("LINK", "display as a hypertext link", 3);
            AddValue("REDIRECT", "act as a redirect to a rule form", 4);
        }
    }
}
