using System;

namespace HomeBot
{
    [Serializable]
    internal class PictureSchedule
    {
        public string Name { get; set; } = "NoName";

        public TimeSpan Interval { get; set; } = TimeSpan.FromMinutes(1);

        [Obsolete("Serialization only!", true)]
        public PictureSchedule() { }

        public PictureSchedule(string name, TimeSpan interval)
        {
            Name = name;
            Interval = interval;
        }
    }
}