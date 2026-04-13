/// <summary>
/// ILayoutNode.cs - Core module for the Darl.dev project.
/// </summary>

﻿using Darl.GraphQL.Models.Models.Noda;

namespace Darl.GraphQL.Process.Web.Models.Noda
{
    public interface ILayoutNode
    {
        string uuid { get; set; }
        NodaPosition position { get; set; }
    }
}
