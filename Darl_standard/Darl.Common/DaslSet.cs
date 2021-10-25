using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarlCommon
{
    /// <summary>
    /// A data set for a simulation
    /// </summary>
    public class DaslSet
    {
        /// <summary>
        /// Gets or sets the events.
        /// </summary>
        /// <value>
        /// The events.
        /// </value>
        [Required]
        [Display(Name = "The sequence of events", Description = "A sequence of time-tagged sets of values")]
        public List<DaslState> events { get; set; } = new List<DaslState>();

        /// <summary>
        /// Gets or sets the sample time.
        /// </summary>
        /// <value>
        /// The sample time.
        /// </value>
        [Required]
        [Display(Name = "The sample time", Description = "Will be used to set up the sample time of the simulation")]
        public TimeSpan sampleTime { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [Display(Name = "The description", Description = "Description of the contained sampled events")]
        public string description { get; set; }
    }
}
