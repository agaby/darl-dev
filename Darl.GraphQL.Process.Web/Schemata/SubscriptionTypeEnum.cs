/// <summary>
/// SubscriptionTypeEnum.cs - Core module for the Darl.dev project.
/// </summary>

﻿using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class SubscriptionTypeEnum : EnumerationGraphType
    {
        public SubscriptionTypeEnum()
        {
            Name = "SubscriptionType";
            Add("individual", 0, "A single user");
            Add("corporate", 1, "A corporate user");
            Add("embedded", 2, "A license to embed DARL nugets or docker instances");
            Add("inhouse", 3, "A license for DARL associates");
        }
    }
}
