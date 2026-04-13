/// </summary>

﻿using System;
using System.Collections.Generic;

namespace Darl.Thinkbase
{
    /// version used for external creation of a KnowledgeState
    /// </summary>
    public class KnowledgeStateInput
    {
        /// the KG it applies to
        /// </summary>
        public string knowledgeGraphName { get; set; }

        /// The individual the state relates to, a Guid
        /// </summary>
        public string subjectId { get; set; }
        /// if true the Knowledge state is not stored, but does trigger any subscriptions.
        /// </summary>
        public bool transient { get; set; } = false;

        /// The data,
        /// organized as GraphObject.Id against the graphAttributes to apply.
        /// </summary>
        public List<StringListGraphAttributeInputPair> data { get; set; } = new List<StringListGraphAttributeInputPair>();

        public KnowledgeState Convert(string userId)
        {
            var ks = new KnowledgeState { created = DateTime.UtcNow, knowledgeGraphName = knowledgeGraphName, subjectId = subjectId, data = new Dictionary<string, List<GraphAttribute>>(), userId = userId };
            foreach (var p in data)
            {
                ks.data.Add(p.name, ConvertAttributeInputList(p.value));
            }
            return ks;
        }

        public static List<GraphAttribute> ConvertAttributeInputList(List<GraphAttributeInput> list)
        {
            var l = new List<GraphAttribute>();
            foreach (var a in list)
                l.Add(ConvertAttributeInput(a));
            return l;
        }

        public static GraphAttribute ConvertAttributeInput(GraphAttributeInput a)
        {
            return new GraphAttribute { id = Guid.NewGuid().ToString(), confidence = a.confidence ?? 1.0, inferred = a.inferred ?? false, value = a.value, existence = a.existence, name = a.name, type = a.type, lineage = a.lineage };
        }
    }

    public class StringListGraphAttributeInputPair
    {
        public string name { get; set; }

        public List<GraphAttributeInput> value { get; set; }
    }
}
