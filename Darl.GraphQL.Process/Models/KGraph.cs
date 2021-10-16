using Darl.Common;
using DarlCommon;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Models
{

    public class KGraph
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string userId { get; set; }

        public bool Shared { get; set; } = false;

        public string OwnerId { get; set; }

        public bool? ReadOnly { get; set; } = false;

        /// <summary>
        /// Initial text for the conversation in demo mode
        /// </summary>
        public string InitialText { get; set; }

        public bool? hidden { get; set; } = false;

    }
}
