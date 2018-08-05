using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using SmashOrPass.Data;
using SmashOrPass.Log;

namespace SmashOrPass
{
    class Program
    {
        static void Main(string[] args)
        {
            string tokenDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\secret.token";
            string token = string.Empty;

            Logger.Log($"Token file location: {tokenDir}", "Init", LogSeverity.Info);

            if (!File.Exists(tokenDir))
            {
                Logger.Log("No token found! Aborting...", "Init", LogSeverity.Critical);
                System.Threading.Thread.Sleep(3000);
            }

            try
            {
                token = File.ReadAllText(tokenDir);
            }
            catch (Exception e)
            {
                Logger.Log($"Exception occured while reading file: {e.Message}", "Init", LogSeverity.Critical);
                return;
            }

            if (string.IsNullOrEmpty(token))
            {
                Logger.Log($"Empty token", "Init", LogSeverity.Critical);
                return;
            }

            Logger.Log($"Token imported", "Init", LogSeverity.Info);

            Bot bot = new Bot(token);
            SmashDatabase.Init();
            Task.Run(bot.StartBot);

            while (true)
            {
                ConsoleCommands.Parse(Console.ReadLine());
            }
        }
    }
}
