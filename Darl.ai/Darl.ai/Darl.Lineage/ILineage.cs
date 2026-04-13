/// <summary>
/// ILineage.cs - Core module for the Darl.dev project.
/// </summary>

﻿namespace Darl.Lineage
{
    public interface ILineage
    {
        bool IsChildOff(Lineage other);
        bool IsParentOf(Lineage other);
    }
}