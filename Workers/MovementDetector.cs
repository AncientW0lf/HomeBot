using System.Timers;
using System;
using System.IO;
using System.Net;
using Discord.WebSocket;
using TcpSubLib;
using System.Threading.Tasks;

namespace HomeBot.Workers
{
    internal class MovementDetector : SettingWatcher<MovementDetectorSetting>, IDiscordWorker
    {
        private DiscordSocketClient _client;

        public override string Filename { get; } = File;

        public const string File = "detector.json";

        private readonly TcpReader _tcp;

        private const string PortFile = "tcpport";

        private Timer _reader = new Timer
        {
            AutoReset = true,
            Interval = 1000
        };

        public MovementDetector()
        {
            ushort port;
            try
            {
                port = ushort.Parse(System.IO.File.ReadAllText(PortFile));
            }
            catch (Exception)
            {
                Console.WriteLine($"Could not parse port file \"{PortFile}\"");
                throw;
            }

            _tcp = new TcpReader(IPAddress.Any, port);

            _reader.Elapsed += async (_, _) => await GetMovement();
        }

        public void StartWork(DiscordSocketClient client)
        {
            _client = client;
            _reader.Start();
        }

        private async Task GetMovement()
        {
            if (!_tcp.Connected)
            {
                _reader.Stop();
                Console.WriteLine($"Movement detector no longer connected.");
                return;
            }

            if (_tcp.AvailableData <= 0)
                return;

            await SendMovementPictures(_tcp.Read());
        }

        private async Task SendMovementPictures(byte[] image)
        {
            for (int i = 0; i < Settings.Length; i++)
            {
                SocketTextChannel channel = _client.GetTextChannel(Settings[i].ChannelPath);
                using var stream = new MemoryStream(image);
                using var typing = channel.EnterTypingState();
                await channel.SendFileAsync(stream, "img.png", $"Movement detected at ca. [{DateTime.UtcNow:u}]!");
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            _tcp.Dispose();
            _reader.Dispose();
        }
    }
}