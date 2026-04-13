/// <summary>
/// Lineage.cs - Core module for the Darl.dev project.
/// </summary>

﻿namespace Darl.Lineage
{
    //wrapper for an ontology
    public class Lineage : ILineage
    {
        private readonly string principal;
        public bool IsParentOf(Lineage other)
        {
            return other.principal.StartsWith(principal);
        }
        public bool IsChildOff(Lineage other)
        {
            return principal.StartsWith(other.principal);
        }
        public override string ToString()
        {
            return principal;
        }
        public override bool Equals(object obj)
        {
            if (obj is string)
                return principal == obj as string;
            return false;
        }
    }
}
