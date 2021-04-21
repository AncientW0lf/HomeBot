using System;
using System.IO;
using System.Text.Json;

namespace HomeBot
{
    internal class PictureScheduler : IDisposable
    {
        public PictureSchedule[] Schedules = new PictureSchedule[0];

        public const string Filename = "picschedules.json";

        private readonly FileSystemWatcher _scheduleWatcher;

        public PictureScheduler()
        {
            if (!File.Exists(Filename))
                File.Create(Filename).Dispose();
            else
                TryReloadSchedules();

            _scheduleWatcher = new FileSystemWatcher(Environment.CurrentDirectory, Filename);
            _scheduleWatcher.Created += (_, _) => TryReloadSchedules();
            _scheduleWatcher.Changed += (_, _) => TryReloadSchedules();
            _scheduleWatcher.EnableRaisingEvents = true;
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

            Console.WriteLine("Reloaded picture schedules.");

            return true;
        }

        public void Dispose()
        {
            _scheduleWatcher.Dispose();
        }
    }
}