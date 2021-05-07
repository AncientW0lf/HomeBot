using System.Threading.Tasks;
using Discord.WebSocket;

namespace HomeBot.Commands
{
    /// <summary>
    /// Provides methods used for Discord commands.
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Executes this command.
        /// </summary>
        /// <param name="msg">The user message that was received and recognized as a command.</param>
        public Task Execute(SocketMessage msg);
    }
}