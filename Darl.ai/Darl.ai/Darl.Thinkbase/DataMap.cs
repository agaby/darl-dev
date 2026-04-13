/// <summary>
/// DataMap.cs - Core module for the Darl.dev project.
/// </summary>

﻿using static Darl.Thinkbase.GraphAttribute;

namespace Darl.Thinkbase
{
    /// <summary>
    /// Definition for mapping input data to the KnowledgeGraph nodes and attributes.
    /// </summary>
    /// <remarks>Lineages will be converted if typewords.</remarks>
    /// <remarks>If connecting to an existing KG, supply relpath, objId and attlineage. If creating a new KG, supply relpath, objId as name, target, dataType, ObjectSubLineage 
    /// and ObjectLineage.</remarks>
    public class DataMap
    {
        /// <summary>
        /// The relative path in XPath or JPath to the data 
        /// </summary>
        public string relPath { get; set; } = string.Empty;

        /// <summary>
        /// The objectId to connect to or the externalId
        /// </summary>
        public string objId { get; set; } = string.Empty;

        /// <summary>
        /// The lineage of the attribute to connect to
        /// </summary>
        public string attLineage { get; set; } = "answer";

        /// <summary>
        /// True if this is the target, i.e the predicted or classified value, of supervised learning
        /// </summary>
        public bool target { get; set; } = false;
        /// <summary>
        /// The type of the loaded value
        /// </summary>
        public DataType dataType { get; set; } = DataType.numeric;
        /// <summary>
        /// The lineage for this object if created
        /// </summary>
        public string objectLineage { get; set; } = string.Empty;
        /// <summary>
        /// The sub lineage for this object if created
        /// </summary>
        public string objectSubLineage { get; set; } = string.Empty;
    }
}
