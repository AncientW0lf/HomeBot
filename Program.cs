using System.Threading;
using System;

namespace HomeBot
{
    internal class Program
    {
        private const string TokenVar = "DiscordToken";

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
            string token = Environment.GetEnvironmentVariable(TokenVar);

            if (string.IsNullOrWhiteSpace(token))
            {
                Console.WriteLine($"Could not get environment variable \"{TokenVar}\".");
                return;
            }

            DiscordClient client;
            try
            {
                client = new DiscordClient(token);
            }
            catch (Exception exc)
            {
                Console.WriteLine($"Could not start bot: {exc.Message}");
                return;
            }

            Console.WriteLine("Started bot.");

            while (!_closing)
                Thread.Sleep(100);

            client.Dispose();
        }

        private static void ExitApp(object sender, ConsoleCancelEventArgs e)
        {
            Console.WriteLine("Terminating bot...");

            _closing = true;

            Thread.Sleep(250);
        }
    }
}
