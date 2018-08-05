using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using SmashOrPass.Data;

namespace SmashOrPass
{
    public static class ConsoleCommands
    {
        public static void Parse(string input)
        {
            string[] data = input.Split(' ');
            data[0] = data[0].ToLower();

            switch (data[0])
            {
                case "score":
                    Score(Convert.ToUInt64(data[1]));
                    break;
                case "listall":
                    ListAll();
                    break;
                case "list":
                    List(data.Skip(1).Select(d => d.Substring(1, d.Length - 2)).ToArray());
                    break;
                case "allowstop":
                    AllowStop(Convert.ToUInt64(data[1]));
                    break;
                case "help":
                    Help();
                    break;
            }
        }

        public static void Score(ulong id)
        {
            foreach (var entry in SmashDatabase.Data)
            {
                if (entry.Key != id) continue;

                DisplayPlayerScore(SmashDatabase.GetEntry(id));
                return;
            }
        }

        public static void ListAll()
        {
            foreach (var entry in SmashDatabase.Data)
            {
                DisplayPlayerScore(SmashDatabase.GetEntry(entry.Key));
            }
        }

        public static void List(string[] userNames)
        {
            foreach (var entry in SmashDatabase.Data)
            {
                if (userNames.Contains(entry.Value.Name))
                {
                    DisplayPlayerScore(SmashDatabase.GetEntry(entry.Key));
                }
            }
        }

        public static void AllowStop(ulong id)
        {
            if (SmashDatabase.HasUser(id))
            {
                SmashDatabase.Data[id].RatedBy.Add(0);
            }
        }

        [Command("help")]
        public static void Help()
        {
            Console.WriteLine(
                "|    help            Shows help screen\n"+
                "|    allowstop {id}  Allows !comfirmstop\n"+
                "|    score {id}      Shows user score\n"+
                "|    list {str[]}    Lists all running SoPs from list\n" +
                "|    listall         Lists all currently running SoPs");
        }

        private static void DisplayPlayerScore(UserEntry entry)
        {
            Console.WriteLine($"|    {entry.Id} ({entry.Name}): {entry.Smashes} S / {entry.Passes} P");
        }
    }
}
