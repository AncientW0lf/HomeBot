using System.Linq;
using System.Timers;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text.Json;
using Unosquare.RaspberryIO;
using System.Threading.Tasks;
using Unosquare.RaspberryIO.Camera;
using Discord.WebSocket;

namespace HomeBot
{
    internal class PictureScheduler : IDisposable
    {
        public PictureSchedule[] Schedules = new PictureSchedule[0];

        public const string Filename = "picschedules.json";

        private readonly DiscordSocketClient _client;

        private readonly FileSystemWatcher _scheduleWatcher;

        private readonly Dictionary<string, Timer> _scheduleExecutors = new Dictionary<string, Timer>();

        public PictureScheduler(DiscordSocketClient client)
        {
            _client = client;

            if (!File.Exists(Filename))
                File.Create(Filename).Dispose();
            else
                TryReloadSchedules();

            _scheduleWatcher = new FileSystemWatcher(Environment.CurrentDirectory, Filename);
            _scheduleWatcher.Created += (_, _) => TryReloadSchedules();
            _scheduleWatcher.Changed += (_, _) => TryReloadSchedules();
            _scheduleWatcher.EnableRaisingEvents = true;
        }

        public async Task TakePicture(string channelPath)
        {
            string guildName = channelPath.Substring(0, channelPath.IndexOf('.'));
            string channelName = channelPath.Substring(channelPath.IndexOf('.') + 1);
            var channel = _client
                .Guilds.FirstOrDefault(a => a.Name.Equals(guildName))
                ?.TextChannels.FirstOrDefault(b => b.Name.Equals(channelName));

            if (channel == null)
            {
                Console.WriteLine($"Could not take picture: invalid channel path \"{channelPath}\".");
                return;
            }

            using (var typing = channel.EnterTypingState())
            {
                byte[] pic = await Pi.Camera.CaptureImageAsync(new CameraStillSettings
                {
                    CaptureEncoding = CameraImageEncodingFormat.Jpg,
                    CaptureTimeoutMilliseconds = 1250,
                    CaptureJpegQuality = 90,
                    CaptureWidth = 640,
                    CaptureHeight = 480,
                    HorizontalFlip = true,
                    VerticalFlip = true
                });
                using var picStream = new MemoryStream(pic);

                await channel.SendFileAsync(picStream, "img.jpg", DateTime.UtcNow.ToString("u"));
            }
        }

        private bool TryReloadSchedules()
        {
            try
            {
                Schedules = JsonSerializer.Deserialize<PictureSchedule[]>(File.ReadAllText(Filename));
            }
            catch (Exception exc)
            {
                Console.WriteLine(
                    "Failed to reload picture schedules.\n" +
                    $"Exception: {exc.Message}");
                return false;
            }

            RestartScheduleTimers();

            Console.WriteLine("Reloaded picture schedules.");

            return true;
        }

        private void RestartScheduleTimers()
        {
            DisposeOldScheduleTimers();
            _scheduleExecutors.Clear();

            foreach (PictureSchedule schedule in Schedules)
            {
                var newTimer = new Timer
                {
                    AutoReset = true,
                    Interval = schedule.Interval.TotalMilliseconds
                };
                newTimer.Elapsed += (_, _) => TakePicture(schedule.ChannelPath).GetAwaiter().GetResult();
                newTimer.Start();
                _scheduleExecutors.Add(schedule.Name, newTimer);
            }
        }

        private void DisposeOldScheduleTimers()
        {
            foreach (var timer in _scheduleExecutors)
                timer.Value.Dispose();
        }

        public void Dispose()
        {
            DisposeOldScheduleTimers();
            _scheduleWatcher.Dispose();
        }
    }
}