using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace HomeBot.Commands
{
    /// <summary>
    /// Takes a picture on user request and sends it to the right channel.
    /// </summary>
    public class TakePictureCommand : ICommand
    {
        public async Task Execute(SocketMessage msg)
        {
            await Program.PicScheduler.TakePicture($"{msg.Author.MutualGuilds.First().Name}.{msg.Channel.Name}");
        }
    }
}