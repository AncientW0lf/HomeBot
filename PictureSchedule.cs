using System;
using System.Text.Json.Serialization;

namespace HomeBot
{
    /// <summary>
    /// Contains information about a schedule that takes pictures.
    /// </summary>
    [Serializable]
    internal class PictureSchedule
    {
        /// <summary>
        /// The name of the schedule to uniquely identify it.
        /// </summary>
        public string Name { get; set; } = "NoName";

        /// <summary>
        /// The full path to the Discord channel to send pictures to.
        /// </summary>
        public string ChannelPath { get; set; } = "Server.Channel";

        /// <summary>
        /// The interval of this schedule.
        /// </summary>
        [JsonConverter(typeof(TimeSpanJsonConverter))]
        public TimeSpan Interval { get; set; } = TimeSpan.FromMinutes(1);

        [Obsolete("Serialization only!", true)]
        public PictureSchedule() { }

        /// <summary>
        /// Creates a new <see cref="PictureSchedule"/>.
        /// </summary>
        /// <param name="name">The name of this schedule.</param>
        /// <param name="channelpath">The full path of the Discord channel to send pictures to.</param>
        /// <param name="interval">The interval of this schedule.</param>
        public PictureSchedule(string name, string channelpath, TimeSpan interval)
        {
            Name = name;
            ChannelPath = channelpath;
            Interval = interval;
        }
    }
}