using System.Linq;
using Discord.WebSocket;

namespace HomeBot
{
    public static class DiscordExtensions
    {
        public static SocketTextChannel GetTextChannel(this DiscordSocketClient client, string channelPath)
        {
            //Gets the name of the server
            string guildName = channelPath.Substring(0, channelPath.IndexOf('.'));

            //Gets the name of the channel
            string channelName = channelPath.Substring(channelPath.IndexOf('.') + 1);

            //Gets the channel object
            return client
                .Guilds.FirstOrDefault(a => a.Name.Equals(guildName))
                ?.TextChannels.FirstOrDefault(b => b.Name.Equals(channelName));
        }

        public static string GetPath(this SocketGuildChannel channel)
        {
            return $"{channel.Guild.Name}.{channel.Name}";
        }
    }
}