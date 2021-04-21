using System.Linq;
using System;
using System.IO;
using System.Threading.Tasks;
using Discord.WebSocket;
using Unosquare.RaspberryIO;
using Unosquare.RaspberryIO.Camera;

namespace HomeBot.Commands
{
    public class TakePictureCommand : ICommand
    {
        public async Task Execute(SocketMessage msg)
        {
            await Program.PicScheduler.TakePicture($"{msg.Author.MutualGuilds.First().Name}.{msg.Channel.Name}");
        }
    }
}