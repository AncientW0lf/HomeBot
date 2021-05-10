using System.IO;
using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using System.Text.Json;
using HomeBot.Workers;

namespace HomeBot.Commands
{
    /// <summary>
    /// Lets the user create and stop schedules to take pictures.
    /// </summary>
    public class SchedulePictureCommand : ICommand
    {
        /// <summary>
        /// The syntax of this command.
        /// </summary>
        private const string Syntax = "SchedulePicture start/stop name [start:interval]";

        public async Task Execute(SocketMessage msg)
        {
            //Gets the command args
            string[] args = msg.Content.Split(' ');

            //Sends a info message that not enough arguments were provided
            if (args.Length < 3)
            {
                await msg.Channel.SendMessageAsync($"Command syntax: {Syntax}");
                return;
            }

            //Gets whether the user wants to start or stop a schedule
            string method = args[1].ToLower();

            switch (method)
            {
                case "start":
                    //Sends a info message that not enough arguments were provided
                    if (args.Length < 4)
                    {
                        await msg.Channel.SendMessageAsync($"Command syntax: {Syntax}");
                        return;
                    }

                    //Tries parsing the given timespan for the new schedule
                    if (TimeSpan.TryParse(args[3], out TimeSpan interv))
                    {
                        //Tries creating the schedule
                        if (TrySetupSchedule(msg, args[2], interv))
                            await msg.Channel.SendMessageAsync($"Started schedule {args[2]} with interval of {interv}.");
                        else
                            await msg.Channel.SendMessageAsync($"Could not create schedule {args[2]}.");
                    }
                    else
                    {
                        //Sends a info message that the given timespan is not valid
                        await msg.Channel.SendMessageAsync($"Could not parse given timespan \"{args[3]}\".");
                        return;
                    }
                    break;
                case "stop":
                    //Tries stopping the schedule
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

        /// <summary>
        /// Tries to set up a new schedule for <see cref="Program.PicScheduler"/>.
        /// </summary>
        /// <param name="msg">The user message.</param>
        /// <param name="name">The name of the schedule.</param>
        /// <param name="interval">The interval of the schedule.</param>
        /// <returns>Returns whether the operation was successful.</returns>
        private bool TrySetupSchedule(SocketMessage msg, string name, TimeSpan interval)
        {
            //Returns false if the given schedule already exists
            if (Program.PicScheduler.Schedules.Count(a => a.Name.Equals(name)) > 0)
                return false;

            //Gets the channel path where the command was received in
            string channelPath = $"{msg.Author.MutualGuilds.First().Name}.{msg.Channel.Name}";

            //Creates a new array with the new schedule appended
            PictureSchedule[] newArr = Program.PicScheduler.Schedules
                .Append(new PictureSchedule(name, channelPath, interval)).ToArray();

            //Tries to write the new schedule to file
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

        /// <summary>
        /// Tries to stop a schedule.
        /// </summary>
        /// <param name="name">The name of the schedule to stop.</param>
        /// <returns>Returns whether the operation was successful.</returns>
        private bool TryStopSchedule(string name)
        {
            //Returns false if the given schedule does not exist
            if (Program.PicScheduler.Schedules.Count(a => a.Name.Equals(name)) == 0)
                return false;

            //Creates a new array with the given schedule removed
            PictureSchedule[] newArr = Program.PicScheduler.Schedules.Where(a => !a.Name.Equals(name)).ToArray();

            //Tries to write the schedules with the given one removed to file
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