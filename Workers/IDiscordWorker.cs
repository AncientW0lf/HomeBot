using Discord.WebSocket;

namespace HomeBot.Workers
{
    public interface IDiscordWorker
    {
        void StartWork(DiscordSocketClient client);
    }
}