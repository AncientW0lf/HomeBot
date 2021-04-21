using System.IO;
using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using System.Text.Json;

namespace HomeBot.Commands
{
    public class SchedulePictureCommand : ICommand
    {
        private const string Syntax = "SchedulePicture start/stop name [start:interval]";

        public async Task Execute(SocketMessage msg)
        {
            string[] args = msg.Content.Split(' ');

            if (args.Length < 3)
            {
                await msg.Channel.SendMessageAsync($"Command syntax: {Syntax}");
                return;
            }

            string method = args[1].ToLower();

            switch (method)
            {
                case "start":
                    if (args.Length < 4)
                    {
                        await msg.Channel.SendMessageAsync($"Command syntax: {Syntax}");
                        return;
                    }

                    if (TimeSpan.TryParse(args[3], out TimeSpan interv))
                    {
                        if (TrySetupSchedule(args[2], interv))
                            await msg.Channel.SendMessageAsync($"Started schedule {args[2]} with interval of {interv}.");
                        else
                            await msg.Channel.SendMessageAsync($"Could not create schedule {args[2]}.");
                    }
                    else
                    {
                        await msg.Channel.SendMessageAsync($"Could not parse given timespan \"{args[3]}\".");
                        return;
                    }
                    break;
                case "stop":
                    if (TryStopSchedule(args[2]))
                        await msg.Channel.SendMessageAsync($"Stopped schedule {args[2]}.");
                    else
                        await msg.Channel.SendMessageAsync($"Could not stop schedule {args[2]}.");
                    break;

                default:
                    await msg.Channel.SendMessageAsync($"Unrecognized method: {method}");
                    break;
            }
        }

        private bool TrySetupSchedule(string name, TimeSpan interval)
        {
            if (PictureScheduler.Instance.Schedules.Count(a => a.Name.Equals(name)) > 0)
                return false;

            PictureSchedule[] newArr = PictureScheduler.Instance.Schedules.Append(new PictureSchedule(name, interval)).ToArray();

            try
            {
                File.WriteAllText(
                    PictureScheduler.Filename,
                    JsonSerializer.Serialize<PictureSchedule[]>(newArr, new JsonSerializerOptions
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

        private bool TryStopSchedule(string name)
        {
            if (PictureScheduler.Instance.Schedules.Count(a => a.Name.Equals(name)) == 0)
                return false;

            PictureSchedule[] newArr = PictureScheduler.Instance.Schedules.Where(a => !a.Name.Equals(name)).ToArray();

            try
            {
                File.WriteAllText(
                    PictureScheduler.Filename,
                    JsonSerializer.Serialize<PictureSchedule[]>(newArr, new JsonSerializerOptions
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