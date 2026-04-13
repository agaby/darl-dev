/// <summary>
/// </summary>

﻿using System.Collections.Generic;

namespace Darl.Thinkbase
{
    /// <summary>
    /// Class to update a knowledge state
    /// </summary>
    public class KnowledgeStateUpdate
    {
        public KnowledgeStateUpdate()
        {

        }

        public KnowledgeStateUpdate(KnowledgeState state)
        {
            data = state.data;
            knowledgeGraphName = state.knowledgeGraphName;
        }

        public Dictionary<string, List<GraphAttribute>> data { get; set; } = new Dictionary<string, List<GraphAttribute>>();

        public string knowledgeGraphName { get; set; }
    }
}
