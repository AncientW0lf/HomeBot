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
            using (var typing = msg.Channel.EnterTypingState())
            {
                byte[] pic = await Pi.Camera.CaptureImageAsync(new CameraStillSettings
                {
                    CaptureEncoding = CameraImageEncodingFormat.Jpg,
                    CaptureTimeoutMilliseconds = 1250,
                    CaptureJpegQuality = 90,
                    CaptureWidth = 640,
                    CaptureHeight = 480,
                    HorizontalFlip = true,
                    VerticalFlip = true
                });
                using var picStream = new MemoryStream(pic);

                await msg.Channel.SendFileAsync(picStream, "img.jpg", DateTime.UtcNow.ToString("u"));
            }
        }
    }
}