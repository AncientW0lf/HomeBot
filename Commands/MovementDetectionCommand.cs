using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Discord.WebSocket;
using HomeBot.Workers;

namespace HomeBot.Commands
{
    public class MovementDetectionCommand : ICommand
    {
        /// <summary>
        /// The syntax of this command.
        /// </summary>
        private const string Syntax = "MovementDetection start/stop";

        public async Task Execute(SocketMessage msg)
        {
            string[] args = msg.Content.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (args.Length < 2)
            {
                await msg.Channel.SendMessageAsync($"Command syntax: {Syntax}");
                return;
            }

            string method = args[1].ToLower();
            switch (method)
            {
                case "start":
                    if (TryAddChannel((msg.Channel as SocketGuildChannel).GetPath()))
                        await msg.Channel.SendMessageAsync("Added this channel to receive motion detection alarms.");
                    else
                        await msg.Channel.SendMessageAsync("Could not add this channel to receive motion detection alarms.");
                    break;
                case "stop":
                    if (TryRemoveChannel((msg.Channel as SocketGuildChannel).GetPath()))
                        await msg.Channel.SendMessageAsync("Removed this channel from receiving motion detection alarms.");
                    else
                        await msg.Channel.SendMessageAsync("Could not remove this channel from receiving motion detection alarms.");
                    break;

                default:
                    await msg.Channel.SendMessageAsync($"Unrecognized method: {method}");
                    break;
            }
        }

        private bool TryAddChannel(string channelPath)
        {
            MovementDetector detector = (MovementDetector)Program.Workers.FirstOrDefault(w => w is MovementDetector);

            //Returns false if the given setting already exists
            if (detector?.Settings.Count(a => a.ChannelPath.Equals(channelPath)) > 0)
                return false;

            //Creates a new array with the new setting appended
            MovementDetectorSetting[] newArr = detector.Settings
                .Append(new MovementDetectorSetting(channelPath)).ToArray();

            //Tries to write the new setting to file
            try
            {
                File.WriteAllText(
                    MovementDetector.File,
                    JsonSerializer.Serialize<MovementDetectorSetting[]>(newArr, new JsonSerializerOptions
                    {
                        WriteIndented = true
                    }));
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private bool TryRemoveChannel(string channelPath)
        {
            MovementDetector detector = (MovementDetector)Program.Workers.FirstOrDefault(w => w is MovementDetector);

            //Returns false if the given setting does not exist
            if (detector.Settings.Count(a => a.ChannelPath.Equals(channelPath)) == 0)
                return false;

            //Creates a new array with the given setting removed
            MovementDetectorSetting[] newArr = detector.Settings.Where(a => !a.ChannelPath.Equals(channelPath)).ToArray();

            //Tries to write the settings with the given one removed to file
            try
            {
                File.WriteAllText(
                    MovementDetector.File,
                    JsonSerializer.Serialize<MovementDetectorSetting[]>(newArr, new JsonSerializerOptions
                    {
                        WriteIndented = true
                    }));
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}