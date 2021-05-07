using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System;
using Discord;
using Discord.WebSocket;
using HomeBot.Commands;

namespace HomeBot
{
    /// <summary>
    /// Wrapper class for <see cref="DiscordSocketClient"/> to easily serve commands as a Discord bot.
    /// </summary>
    internal class DiscordClient : IDisposable
    {
        /// <summary>
        /// The underlying client object to use for communication to Discord.
        /// </summary>
        public DiscordSocketClient Client;

        /// <summary>
        /// The prefix to use for all commands to identify them in chat.
        /// </summary>
        private const char CommandPrefix = '/';

        /// <summary>
        /// The list of commands that can be executed by a user.
        /// </summary>
        private ICommand[] _commands;

        /// <summary>
        /// Creates a new <see cref="DiscordClient"/>, automatically connects it Discord and adds all commands that can be executed.
        /// </summary>
        /// <param name="token">The Discord token to authenticate to Discord.</param>
        public DiscordClient(string token)
        {
            AddCommands();

            Client = new DiscordSocketClient();

            Client.MessageReceived += ReceivedMessage;

            Task.Run(async () =>
            {
                await Client.LoginAsync(TokenType.Bot, token);

                await Client.SetGameAsync("you", null, ActivityType.Watching);

                await Client.StartAsync();
            }).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Adds all commands in the <see cref="Commands"/> namespace.
        /// </summary>
        private void AddCommands()
        {
            //Gets all commands
            Type[] commandTypes = Assembly
                .GetExecutingAssembly()
                .GetTypes()
                .Where(t =>
                    t.Namespace.Equals($"{nameof(HomeBot)}.{nameof(Commands)}")
                    && t.Name.EndsWith("Command")
                    && t.IsClass
                    && t.IsAssignableTo(typeof(ICommand)))
                .ToArray();

            //Goes through all commands and instantiates them
            _commands = new ICommand[commandTypes.Length];
            for (int i = 0; i < commandTypes.Length; i++)
                _commands[i] = (ICommand)Activator.CreateInstance(commandTypes[i]);
        }

        /// <summary>
        /// Processes one user message and executes a matching command.
        /// </summary>
        /// <param name="msg">The user message that was received.</param>
        private async Task ReceivedMessage(SocketMessage msg)
        {
            //Ignores the message if it is not a command
            if (!msg.Content.StartsWith(CommandPrefix))
                return;

            try
            {
                //Gets the string position where the command name ends
                int cmdEndIndex = msg.Content.IndexOf(' ') - 1;
                if (cmdEndIndex < 0)
                    cmdEndIndex = msg.Content.Length - 1;

                //Gets the command name
                string cmdName = msg.Content.Substring(1, cmdEndIndex);

                //Gets the matching command
                ICommand matchedCmd = _commands.FirstOrDefault(c =>
                {
                    string cName = c.GetType().Name;
                    cName = cName.Substring(0, cName.LastIndexOf("Command"));

                    return cName.ToLower() == cmdName.ToLower();
                });

                //Executes the matched command
                if (matchedCmd != null)
                {
                    Console.WriteLine($"Matched command: {cmdName}");
                    await matchedCmd.Execute(msg);
                }
            }
            catch (Exception exc)
            {
                await msg.Channel.SendMessageAsync(
                    $"Could not process message with content \"{msg.Content}\"!\n" +
                    $"Exception: {exc.Message} [in {exc.TargetSite?.Name}]");
            }
        }

        public void Dispose()
        {
            Client.Dispose();
        }
    }
}