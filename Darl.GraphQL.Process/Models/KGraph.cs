using Darl.Common;
using DarlCommon;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Models
{
    public enum DateDisplay {recent,historic };
    public enum InferenceTime { now,@fixed};
    public class KGraph
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateDisplay? dateDisplay { get; set; } = DateDisplay.recent;
        public InferenceTime? inferenceTime { get; set; } = InferenceTime.now;
        public DarlTime? fixedTime { get; set; }

        public string userId { get; set; }

        public bool Shared { get; set; } = false;

        public string OwnerId { get; set; }

        public bool? ReadOnly { get; set; } = false;

        /// <summary>
        /// Initial text for the conversation in demo mode
        /// </summary>
        public string InitialText { get; set; }

        public bool? hidden { get; set; } = false;

        #region deprecated
        public List<Authorization> Authorizations { get; set; } = new List<Authorization>();
        public ServiceConnectivity serviceConnectivity { get; set; } = new ServiceConnectivity();
        public List<UserUsage> UsageHistory { get; set; } = new List<UserUsage>();
        #endregion
    }
}
