using System.Threading;
using System;
using HomeBot.Workers;
using Discord.WebSocket;
using System.Reflection;
using System.Linq;

namespace HomeBot
{
    /// <summary>
    /// Entry point of this application.
    /// </summary>
    internal class Program
    {
        public static IDiscordWorker[] Workers { get; private set; }

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

            DisposeWorkers();
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

            InitializeWorkers(client.Client);

            //Waits for the application to receive a close signal
            while (!_closing)
                Thread.Sleep(100);

            client.Dispose();
        }

        private static void InitializeWorkers(DiscordSocketClient client)
        {
            //Gets all workers
            Type[] workerTypes = Assembly
                .GetExecutingAssembly()
                .GetTypes()
                .Where(t =>
                    t.Namespace.Equals($"{nameof(HomeBot)}.{nameof(Workers)}")
                    && t.IsClass
                    && t.IsAssignableTo(typeof(IDiscordWorker)))
                .ToArray();

            //Goes through all commands and instantiates them
            Workers = new IDiscordWorker[workerTypes.Length];
            for (int i = 0; i < workerTypes.Length; i++)
            {
                Workers[i] = (IDiscordWorker)Activator.CreateInstance(workerTypes[i]);
                Workers[i].StartWork(client);
            }
        }

        private static void DisposeWorkers()
        {
            for (int i = 0; i < Workers.Length; i++)
                if (Workers[i] is IDisposable disposableWorker)
                    disposableWorker.Dispose();
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
