using System.Threading.Tasks;
using Discord.WebSocket;

namespace HomeBot.Commands
{
    public interface ICommand
    {
        public Task Execute(SocketMessage msg);
    }
}