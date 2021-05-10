using System.Collections.Generic;
using System;
using System.IO;
using System.Text.Json;
using Unosquare.RaspberryIO;
using System.Threading.Tasks;
using Unosquare.RaspberryIO.Camera;
using Discord.WebSocket;
using System.Threading;
using Timer = System.Timers.Timer;
using HomeBot.Workers.Extensions;

namespace HomeBot.Workers
{
    /// <summary>
    /// Takes pictures based on a schedule defined in <see cref="Filename"/>.
    /// </summary>
    internal class PictureScheduler : IDiscordWorker, IDisposable
    {
        /// <summary>
        /// Contains all defined schedules.
        /// </summary>
        public PictureSchedule[] Schedules = new PictureSchedule[0];

        /// <summary>
        /// The path to the file that contains schedule information.
        /// </summary>
        public const string Filename = "picschedules.json";

        /// <summary>
        /// The client used to send the pictures to a channel.
        /// </summary>
        private DiscordSocketClient _client;

        /// <summary>
        /// File watcher for <see cref="Filename"/>.
        /// </summary>
        private readonly FileSystemWatcher _scheduleWatcher = new FileSystemWatcher(Environment.CurrentDirectory, Filename);

        /// <summary>
        /// Dictionary of <see cref="Timer"/> objects that take pictures on their specified schedule.
        /// </summary>
        private readonly Dictionary<string, Timer> _scheduleExecutors = new Dictionary<string, Timer>();

        public void StartWork(DiscordSocketClient client)
        {
            _client = client;

            if (!File.Exists(Filename))
                File.Create(Filename).Dispose();
            else
                TryReloadSchedules();

            _scheduleWatcher.Created += (_, _) => TryReloadSchedules();
            _scheduleWatcher.Changed += (_, _) => TryReloadSchedules();
            _scheduleWatcher.EnableRaisingEvents = true;
        }

        /// <summary>
        /// Takes a picture and sends it to a Discord channel.
        /// </summary>
        /// <param name="channelPath">The full path to the channel to send the picture to.</param>
        public async Task TakePicture(string channelPath)
        {
            SocketTextChannel channel = _client.GetTextChannel(channelPath);

            //Returns if no channel with the given path could be found
            if (channel == null)
            {
                Console.WriteLine($"Could not take picture: invalid channel path \"{channelPath}\".");
                return;
            }

            //Starts "typing" in the given channel
            using (var typing = channel.EnterTypingState())
            {
                //Creates a mutex to ensure thread synchronisation between processes
                using var m = new Mutex(true, $"{nameof(Pi)}.{nameof(Pi.Camera)}");

                //Waits 10 seconds to be allowed to take a picture
                if (m.WaitOne(TimeSpan.FromSeconds(10)))
                {
                    //Takes a picture and saves the bytes
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

                    //Sends the image bytes to the Discord channel
                    await channel.SendFileAsync(picStream, "img.jpg", DateTime.UtcNow.ToString("u"));

                    //Frees the mutex for other waiting processes
                    m.ReleaseMutex();
                }
            }
        }

        /// <summary>
        /// Tries to reload the schedules from file.
        /// </summary>
        /// <returns>Returns true if the operation was successful.</returns>
        private bool TryReloadSchedules()
        {
            try
            {
                //Reads and deserializes the file
                Schedules = JsonSerializer.Deserialize<PictureSchedule[]>(File.ReadAllText(Filename));
            }
            catch (Exception exc)
            {
                Console.WriteLine(
                    "Failed to reload picture schedules.\n" +
                    $"Exception: {exc.Message}");
                return false;
            }

            //Restarts the timer objects for the schedules
            RestartScheduleTimers();

            Console.WriteLine("Reloaded picture schedules.");

            return true;
        }

        /// <summary>
        /// Restarts all <see cref="Timer"/> objects in <see cref="_scheduleExecutors"/>.
        /// </summary>
        private void RestartScheduleTimers()
        {
            //Disposes and clears all timers
            DisposeOldScheduleTimers();
            _scheduleExecutors.Clear();

            //Adds new timers for every schedule
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

        /// <summary>
        /// Disposes all <see cref="Timer"/> objects in <see cref="_scheduleExecutors"/>.
        /// </summary>
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