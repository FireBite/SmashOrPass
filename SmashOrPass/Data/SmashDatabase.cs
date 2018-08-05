using System.Collections.Generic;
using System.IO;
using System.Timers;
using Discord;
using Newtonsoft.Json;
using SmashOrPass.Log;

namespace SmashOrPass.Data
{
    public static class SmashDatabase
    {
        public static Dictionary<ulong, UserEntry> Data { get; private set; }

        private static Timer timer = new Timer(20000);
        private static string currentDir;

        public static void Init()
        {
            currentDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\SoPDB.json";
            Logger.Log("DB save path: " + currentDir, "SmashDB", LogSeverity.Info);

            if (File.Exists(currentDir))
            {
                Logger.Log("Restoring data from db file", "SmashDB", LogSeverity.Info);
                Data = JsonConvert.DeserializeObject<Dictionary<ulong, UserEntry>>(
                    File.ReadAllText(currentDir));
            }
            else
            {
                File.WriteAllText(currentDir, string.Empty);
                Data = new Dictionary<ulong, UserEntry>();
            }

            timer.Elapsed += SaveToFile;
            timer.Start();
            Logger.Log("Initialized", "SmashDB", LogSeverity.Info);
        }

        public static void AddEntry(UserEntry entry)
        {
            if (Data.ContainsKey(entry.Id))
            {
                Data[entry.Id] = entry;
            }
            else
            {
                Data.Add(entry.Id, entry);
            }

            Logger.Log($"Added user {entry.Id}", "SmashDB", LogSeverity.Info);
        }

        public static UserEntry GetEntry(ulong id)
        {
            return Data.ContainsKey(id) ? Data[id] : null;
        }

        public static bool HasUser(ulong id)
        {
            return Data.ContainsKey(id);
        }

        public static void UpdateEntry(UserEntry entry)
        {
            Data[entry.Id] = entry;
            Logger.Log($"Updated user: {entry.Id} (S: {entry.Smashes}, P: {entry.Passes})", "SmashDB", LogSeverity.Info);
        }

        public static void RemoveEntry(ulong id)
        {
            if (Data.ContainsKey(id))
            {
                Data.Remove(id);
            }
        }

        private static void SaveToFile(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            lock (Data)
            {
                string json = JsonConvert.SerializeObject(Data);

                if (json != File.ReadAllText(currentDir))
                {
                    File.WriteAllText(currentDir, json);
                    Logger.Log("Saved db", "SmashDB", LogSeverity.Info);
                }
            }
        }
    }
}
