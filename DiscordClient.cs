using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System;
using Discord;
using Discord.WebSocket;
using HomeBot.Commands;

namespace HomeBot
{
    internal class DiscordClient : IDisposable
    {
        private const char CommandPrefix = '/';

        private DiscordSocketClient _client;

        private ICommand[] _commands;

        public DiscordClient(string token)
        {
            AddCommands();

            _client = new DiscordSocketClient();

            _client.MessageReceived += ReceivedMessage;

            Task.Run(async () =>
            {
                await _client.LoginAsync(TokenType.Bot, token);

                await _client.SetGameAsync("you", null, ActivityType.Watching);

                await _client.StartAsync();
            }).GetAwaiter().GetResult();
        }

        private void AddCommands()
        {
            Type[] commandTypes = Assembly
                .GetExecutingAssembly()
                .GetTypes()
                .Where(t =>
                    t.Name.EndsWith("Command")
                    && t.IsClass
                    && t.IsAssignableTo(typeof(ICommand)))
                .ToArray();

            _commands = new ICommand[commandTypes.Length];
            for (int i = 0; i < commandTypes.Length; i++)
                _commands[i] = (ICommand)Activator.CreateInstance(commandTypes[i]);
        }

        private async Task ReceivedMessage(SocketMessage msg)
        {
            if (!msg.Content.StartsWith(CommandPrefix))
                return;

            try
            {
                int cmdEndIndex = msg.Content.IndexOf(' ') - 1;
                if (cmdEndIndex < 0)
                    cmdEndIndex = msg.Content.Length - 1;
                string cmdName = msg.Content.Substring(1, cmdEndIndex);

                var matchedCmd = _commands.FirstOrDefault(c =>
                {
                    string cName = c.GetType().Name;
                    cName = cName.Substring(0, cName.LastIndexOf("Command"));

                    return cName.ToLower() == cmdName.ToLower();
                });

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
            _client.Dispose();
        }
    }
}