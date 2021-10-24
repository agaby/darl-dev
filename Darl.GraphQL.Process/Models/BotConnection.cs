using MongoDB.Bson;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Darl.GraphQL.Models.Models
{
    /// <summary>
    /// Two roles - part of the general data structure representing a bot, also rapid lookup for botmodel from appid
    /// </summary>
    public class BotConnection
    {
        /// <summary>
        /// MongoDB reference to this object
        /// </summary>
        public ObjectId id { get; set; }

        [Display(Name = "BotFramework AppId", Description = "This holds the AppID assigned by the MS Bot framework for this bot")]
        [Required]
        public string AppId { get; set; }

        [Display(Name = "BotFramework password", Description = "This holds the password assigned by the MS Bot framework for this bot")]
        [DataType(DataType.Password)]
        [Required]
        public string Password { get; set; }

        public List<UserUsage> UsageHistory { get; set; } = new List<UserUsage>();

        /// <summary>
        /// MongoDB reference to the owning model
        /// </summary>
        public ObjectId botModel { get; set; }
        /// <summary>
        /// reference name of bot/model combination i.e. facebookBot1
        /// </summary>
        public string FriendlyName { get; set; }
        /// <summary>
        /// UserId of owner - copied from model for speed. Not expected to change over the lifetime of a botconnection.
        /// </summary>
        public string userId { get; set; }

    }
}
