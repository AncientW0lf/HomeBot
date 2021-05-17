using System;

namespace HomeBot
{
    [Serializable]
    public class MovementDetectorSetting
    {
        public string ChannelPath { get; set; }

        [Obsolete("Serialization only!", true)]
        public MovementDetectorSetting() { }

        /// <summary>
        /// Creates a new <see cref="MovementDetectorSetting"/>.
        /// </summary>
        /// <param name="channelpath">The full path of the Discord channel to send pictures to.</param>
        public MovementDetectorSetting(string channelpath)
        {
            ChannelPath = channelpath;
        }
    }
}