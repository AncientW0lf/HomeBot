using System.Threading.Tasks;
using System;
using Discord;
using Discord.WebSocket;

namespace HomeBot
{
    internal class DiscordClient : IDisposable
    {
        private DiscordSocketClient _client;

        public DiscordClient(string token)
        {
            _client = new DiscordSocketClient();

            _client.MessageReceived += ReceivedMessage;

            Task.Run(async () =>
            {
                await _client.LoginAsync(TokenType.Bot, token);

                await _client.SetGameAsync("you", null, ActivityType.Watching);

                await _client.StartAsync();
            }).GetAwaiter().GetResult();
        }

        private async Task ReceivedMessage(SocketMessage msg)
        {
            await msg.AddReactionAsync(new Emoji("🍓"));
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}