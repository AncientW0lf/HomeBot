using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace HomeBot.Commands
{
    public class HelpCommand : ICommand
    {
        public async Task Execute(SocketMessage msg)
        {
            //Gets all commands
            string[] commands = Assembly
                .GetExecutingAssembly()
                .GetTypes()
                .Where(t =>
                    t.Namespace.Equals($"{nameof(HomeBot)}.{nameof(Commands)}")
                    && t.Name.EndsWith("Command")
                    && t.IsClass
                    && t.IsAssignableTo(typeof(ICommand)))
                .Select(c => string.Join("", c.Name.SkipLast("Command".Length)))
                .ToArray();

            await msg.Channel.SendMessageAsync($"Available commands:\n" + commands);
        }
    }
}