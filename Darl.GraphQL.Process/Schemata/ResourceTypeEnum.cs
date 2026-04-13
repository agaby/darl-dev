/// <summary>
/// </summary>

﻿using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public enum ResourceType { ruleset, mlmodel, botmodel, document, collateral, simulation }
    public class ResourceTypeEnum : EnumerationGraphType
    {
        public ResourceTypeEnum()
        {
            Name = "resourceTypes";
            Add("ruleset", 0, "A ruleset model");
            Add("mlmodel", 1, "A machine learning model");
            Add("botmodel", 2, "A chatbot model");
            Add("document", 3, "A document");
            Add("collateral", 4, "A piece of collateral");
            Add("simulation", 5, "A simulation model");
        }
    }
}
