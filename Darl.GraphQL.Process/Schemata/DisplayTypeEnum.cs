/// <summary>
/// </summary>

﻿using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class DisplayTypeEnum : EnumerationGraphType
    {

        public DisplayTypeEnum()
        {
            Name = "displayTypes";
            Description = "The ways in which a result can be presented.";
            Add("TEXT", 1, "display as text");
            Add("SCOREBAR", 2, "display as a score bar");
            Add("LINK", 3, "display as a hypertext link");
            Add("REDIRECT", 4, "act as a redirect to a rule form");
        }
    }
}
