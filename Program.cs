using System.Threading;
using System;

namespace HomeBot
{
    /// <summary>
    /// Entry point of this application.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// Background runner to take pictures on a schedule.
        /// </summary>
        public static PictureScheduler PicScheduler { get; private set; }

        /// <summary>
        /// Name of the environment variable to load the Discord token from.
        /// </summary>
        private const string TokenVar = "DiscordToken";

        /// <summary>
        /// This is the <see cref="Thread"/> the discord bot will run in.
        /// </summary>
        private static readonly Thread _botThread = new Thread(StartBot) { Name = "BotMain" };

        /// <summary>
        /// Is this variable set to true, this application will terminate at the next possible point in time.
        /// </summary>
        private static bool _closing;

        /// <summary>
        /// Entry point that starts <see cref="_botThread"/>.
        /// </summary>
        private static void Main()
        {
            Console.CancelKeyPress += ExitApp;

            _botThread.Start();
            _botThread.Join();
        }

        /// <summary>
        /// Entry point for <see cref="_botThread"/> which starts the Discord bot.
        /// </summary>
        private static void StartBot()
        {
            //Gets the bot token
            string token = Environment.GetEnvironmentVariable(TokenVar);

            //Returns if no token could be found
            if (string.IsNullOrWhiteSpace(token))
            {
                Console.WriteLine($"Could not get environment variable \"{TokenVar}\".");
                return;
            }

            Console.WriteLine("Starting bot...");

            //Tries to start the bot
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

            //Initializes the picture scheduler to take pictures
            PicScheduler = new PictureScheduler(client.Client);

            //Waits for the application to receive a close signal
            while (!_closing)
                Thread.Sleep(100);

            client.Dispose();
        }

        /// <summary>
        /// Sets <see cref="_closing"/> to true to close any dependant threads.
        /// </summary>
        private static void ExitApp(object sender, ConsoleCancelEventArgs e)
        {
            Console.WriteLine("Terminating bot...");

            _closing = true;

            Thread.Sleep(250);
        }
    }
}
