using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using HomeBot.Workers;

namespace HomeBot.Commands
{
    /// <summary>
    /// Takes a picture on user request and sends it to the right channel.
    /// </summary>
    public class TakePictureCommand : ICommand
    {
        public async Task Execute(SocketMessage msg)
        {
            await ((PictureScheduler)Program.Workers.FirstOrDefault(w => w is PictureScheduler))
                .TakePictureAsync($"{msg.Author.MutualGuilds.First().Name}.{msg.Channel.Name}");
        }
    }
}