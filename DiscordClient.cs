using System;
using Discord.WebSocket;

namespace HomeBot
{
    internal class DiscordClient : IDisposable
    {
        private DiscordSocketClient _client;

        public DiscordClient()
        {
            _client = new DiscordSocketClient();
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}