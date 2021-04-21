using System;
using System.Text.Json.Serialization;

namespace HomeBot
{
    [Serializable]
    internal class PictureSchedule
    {
        public string Name { get; set; } = "NoName";

        public string ChannelPath { get; set; } = "Server.Channel";

        [JsonConverter(typeof(TimeSpanJsonConverter))]
        public TimeSpan Interval { get; set; } = TimeSpan.FromMinutes(1);

        [Obsolete("Serialization only!", true)]
        public PictureSchedule() { }

        public PictureSchedule(string name, string channelpath, TimeSpan interval)
        {
            Name = name;
            ChannelPath = channelpath;
            Interval = interval;
        }
    }
}