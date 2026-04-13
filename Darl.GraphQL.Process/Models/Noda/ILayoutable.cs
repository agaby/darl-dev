/// <summary>
/// ILayoutable.cs - Core module for the Darl.dev project.
/// </summary>

﻿using System.Collections.Generic;

namespace Darl.GraphQL.Process.Web.Models.Noda
{
    public interface ILayoutable
    {
        ILayoutLink? GetEdge(string fromNode, string toNode);
        ILayoutNode? GetNode(string uuid);
        List<ILayoutNode> GetNodes();
        List<ILayoutLink> GetLinks();
        void Init();
    }
}
