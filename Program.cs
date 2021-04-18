using System.Threading;
using System;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace HomeBot
{
    internal class Program
    {
        private static Thread _botThread = new Thread(StartBot) { Name = "BotMain" };

        private static bool _closing;

        private static void Main()
        {
            Console.CancelKeyPress += ExitApp;

            _botThread.Start();
            _botThread.Join();
        }

        private static void StartBot()
        {
            using var client = new DiscordClient();

            Console.WriteLine("Started bot.");

            while (!_closing)
                Thread.Sleep(100);
        }

        private static void ExitApp(object sender, ConsoleCancelEventArgs e)
        {
            Console.WriteLine("Terminating bot...");

            _closing = true;

            Thread.Sleep(250);
        }
    }
}
