using System;
using System.IO;
using System.Text.Json;

namespace HomeBot
{
    internal abstract class SettingWatcher<SET> : IDisposable
    {
        public SET[] Settings = new SET[0];

        public abstract string Filename { get; }

        protected readonly FileSystemWatcher _settingWatcher;

        protected SettingWatcher()
        {
            _settingWatcher = new FileSystemWatcher(Environment.CurrentDirectory, Filename);

            if (!File.Exists(Filename))
                File.Create(Filename).Dispose();
            else
                TryReloadSettings();

            _settingWatcher.Created += (_, _) => TryReloadSettings();
            _settingWatcher.Changed += (_, _) => TryReloadSettings();
            _settingWatcher.EnableRaisingEvents = true;
        }

        protected virtual bool TryReloadSettings()
        {
            try
            {
                //Reads and deserializes the file
                Settings = JsonSerializer.Deserialize<SET[]>(File.ReadAllText(Filename));
            }
            catch (Exception exc)
            {
                Console.WriteLine(
                    $"Failed to reload {this.GetType().Name} settings.\n" +
                    $"Exception: {exc.Message}");
                return false;
            }

            Console.WriteLine($"Reloaded {this.GetType().Name} settings.");

            return true;
        }

        public virtual void Dispose()
        {
            _settingWatcher.Dispose();
        }
    }
}