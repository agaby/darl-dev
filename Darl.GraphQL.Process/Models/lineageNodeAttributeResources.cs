using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Models
{
    /// <summary>
    /// Contains the resources that are needed to edit or create a lineageNodeAttribute
    /// </summary>
    public class LineageNodeAttributeResources
    {
        public string ruleSkeleton { get; set; }

        public string insertionPointText { get; set; }

        public List<string> AllRulesets { get; set; }

        public List<string> AllRoles { get; set; }

    }
}
