using System;
using System.Web;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using SmashOrPass.Data;
using SmashOrPass.Log;

namespace SmashOrPass.Modules
{
    public class SmashOrPass : ModuleBase<SocketCommandContext>
    {
        [Command("start")]
        public async Task Start([Optional]string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                if (Context.Message.Attachments.Count > 0)
                {
                    url = Context.Message.Attachments.First().Url;
                }
                else
                {
                    Logger.Log("No photo attached!", "StartSoP", LogSeverity.Error);
                    await Context.Channel.SendMessageAsync("Send me your photo to start!");
                    return;
                }
            }

            if (!IsImage(url))
            {
                Logger.Log($"{MimeTypes.GetMimeType(url)} is not a photo!", "StartSoP", LogSeverity.Error);
                await Context.Channel.SendMessageAsync("Send me your photo to start!");
                return;
            }

            var embed = new EmbedBuilder();
            embed.WithTitle("Smash Or Pass");
            embed.WithDescription($"{Context.User.Username} has started Smash Or Pass!");
            embed.AddInlineField($"!smash @{Context.User.Username}", "Vote to smash");
            embed.AddInlineField($"!pass @{Context.User.Username}", "Vote to pass");
            embed.WithImageUrl(url);
            embed.WithColor(new Color(104, 44, 191));

            await Context.Channel.SendMessageAsync("", false, embed);

            Logger.Log($"Starting SoP on user {Context.User.Username}", "StartSoP", LogSeverity.Info);
            SmashDatabase.AddEntry(new UserEntry(){Id = Context.User.Id, Url = url, Name = Context.User.Username});
        }

        [Command("pass")]
        public async Task Pass([Optional]string target)
        {
            if (string.IsNullOrEmpty(target))
            {
                Logger.Log("No user specified", "PassSoP", LogSeverity.Error);
                await Context.Channel.SendMessageAsync("Tell me who you want to pass by writing !pass {@user}");
                return;
            }

            ulong id = Convert.ToUInt64(target.Substring(2, target.Length - 3).Replace("!", string.Empty));
            UserEntry entry = null;

            if (!SmashDatabase.HasUser(id))
            {
                Logger.Log("User not in database!", "PassSoP", LogSeverity.Error);
                await Context.Channel.SendMessageAsync("This user did not start his Smash Or Pass!");
                return;
            }

            entry = SmashDatabase.GetEntry(id);

            if (entry.RatedBy.Contains(Context.User.Id))
            {
                Logger.Log("User has already voted!", "PassSoP", LogSeverity.Error);
                await Context.Channel.SendMessageAsync($"You have already rated {entry.Name}. To see current results write: !score @{entry.Name}");
                return;
            }

            entry.Passes++;
            entry.RatedBy.Add(Context.User.Id);
            SmashDatabase.UpdateEntry(entry);

            var embed = new EmbedBuilder();
            embed.WithTitle("Smash Or Pass");
            embed.WithDescription($"{Context.User.Username} passed {entry.Name}!");
            embed.AddInlineField(entry.Smashes.ToString(), "Total Smashes");
            embed.AddInlineField(entry.Passes.ToString(), "Total Passes");
            embed.WithImageUrl(entry.Url);
            embed.WithColor(new Color(12, 212, 00));

            await Context.Channel.SendMessageAsync("", false, embed);

            Logger.Log($"User {Context.User.Username} passed {entry.Name}", "PassSoP", LogSeverity.Info);
        }

        [Command("smash")]
        public async Task Smash([Optional]string target)
        {
            if (string.IsNullOrEmpty(target))
            {
                Logger.Log("No user specified", "SmashSoP", LogSeverity.Error);
                await Context.Channel.SendMessageAsync("Tell me who you want to smash by writing !smash {@user}");
                return;
            }

            ulong id = Convert.ToUInt64(target.Substring(2, target.Length - 3).Replace("!", string.Empty));
            UserEntry entry = null;

            if (!SmashDatabase.HasUser(id))
            {
                Logger.Log("User not in database!", "SmashSoP", LogSeverity.Error);
                await Context.Channel.SendMessageAsync("This user did not start his Smash Or Pass!");
                return;
            }

            entry = SmashDatabase.GetEntry(id);

            if (entry.RatedBy.Contains(Context.User.Id))
            {
                Logger.Log("User has already voted!", "SmashSoP", LogSeverity.Error);
                await Context.Channel.SendMessageAsync($"You have already rated {entry.Name}. To see current results write: !score @{entry.Name}");
                return;
            }

            entry.Smashes++;
            entry.RatedBy.Add(Context.User.Id);
            SmashDatabase.UpdateEntry(entry);

            var embed = new EmbedBuilder();
            embed.WithTitle("Smash Or Pass");
            embed.WithDescription($"{entry.Name} smashed {entry.Name}!");
            embed.AddInlineField(entry.Smashes.ToString(), "Total Smashes");
            embed.AddInlineField(entry.Passes.ToString(), "Total Passes");
            embed.WithImageUrl(entry.Url);
            embed.WithColor(new Color(0, 165, 249));

            await Context.Channel.SendMessageAsync("", false, embed);

            Logger.Log($"User {Context.User.Username} smashed {entry.Name}", "SmashSoP", LogSeverity.Info);
        }

        [Command("score")]
        public async Task Score([Optional]string target)
        {
            if (string.IsNullOrEmpty(target))
            {
                Logger.Log("No user specified", "ScoreSoP", LogSeverity.Error);
                await Context.Channel.SendMessageAsync("Tell me whose score you want to see by writing !score {@user}");
                return;
            }

            ulong id = Convert.ToUInt64(target.Substring(2, target.Length - 3));
            UserEntry entry = null;

            if (!SmashDatabase.HasUser(id))
            {
                Logger.Log("User not in database!", "ScoreSoP", LogSeverity.Error);
                await Context.Channel.SendMessageAsync("This user did not start his Smash Or Pass!");
                return;
            }

            entry = SmashDatabase.GetEntry(id);

            var embed = new EmbedBuilder();
            embed.WithTitle("Smash Or Pass");
            embed.WithDescription($"{entry.Name} score");
            embed.AddInlineField(entry.Smashes.ToString(), "Smashes");
            embed.AddInlineField(entry.Passes.ToString(), "Passes");
            embed.WithImageUrl(entry.Url);
            embed.WithColor(new Color(104, 44, 191));

            await Context.Channel.SendMessageAsync("", false, embed);

            Logger.Log($"Show {entry.Name} score", "ScoreSoP", LogSeverity.Info);
        }

        [Command("listall")]
        public async Task ListAll()
        {
            var embed = new EmbedBuilder();
            embed.WithTitle("Smash Or Pass");
            embed.WithDescription("All scores");
            foreach (var entry in SmashDatabase.Data)
            {
                embed.AddField(entry.Value.Name, $"S:   {entry.Value.Smashes}\nP:   {entry.Value.Passes}");
            }
            embed.WithColor(new Color(104, 44, 191));

            await Context.Channel.SendMessageAsync("", false, embed);

            Logger.Log("Listed all SoPs", "ListSoP", LogSeverity.Info);
        }

        [Command("stop")]
        public async Task Stop()
        {
            ulong id = Context.User.Id;
            UserEntry entry = null;

            if (!SmashDatabase.HasUser(id))
            {
                Logger.Log("User not in database!", "StopSoP", LogSeverity.Error);
                await Context.Channel.SendMessageAsync("You can't end something, that didn't start!");
                return;
            }

            entry = SmashDatabase.GetEntry(id);
            short score = 0;

            if (entry.Smashes != 0)
            {
                score = (short)Math.Round(entry.Smashes / (decimal)(entry.Smashes + entry.Passes) * 100, 0, MidpointRounding.AwayFromZero);
            }

            var embed = new EmbedBuilder();
            embed.WithTitle("Smash Or Pass");
            embed.WithDescription($"{Context.User.Username} ends his game!");
            embed.AddInlineField(entry.Smashes.ToString(), "Smashes");
            embed.AddInlineField(entry.Passes.ToString(), "Passes");
            embed.AddField(CommentScore(score, entry.Name, score < 50 ? "passed" : "smashed"), $"({score}%)");
            embed.WithImageUrl(entry.Url);
            embed.WithColor(score > 50 ? new Color(0, 165, 249) : new Color(212, 12, 00));

            await Context.Channel.SendMessageAsync("", false, embed);

            SmashDatabase.RemoveEntry(entry.Id);
            Logger.Log($"Ending SoP on user {entry.Name}", "EndSoP", LogSeverity.Info);
        }

        [Command("confirmstop")]
        public async Task ConfirmStop(ulong id)
        {
            UserEntry entry = SmashDatabase.GetEntry(id);
            short score = 0;

            if (!entry.RatedBy.Contains(0))
            {
                Logger.Log("User not marked!", "ConfirmStopSoP", LogSeverity.Error);
            }

            if (entry.Smashes != 0)
            {
                score = (short)Math.Round(entry.Smashes / (decimal)(entry.Smashes + entry.Passes) * 100, 0, MidpointRounding.AwayFromZero);
            }

            var embed = new EmbedBuilder();
            embed.WithTitle("Smash Or Pass");
            embed.WithDescription($"{entry.Name} ends his game!");
            embed.AddInlineField(entry.Smashes.ToString(), "Smashes");
            embed.AddInlineField(entry.Passes.ToString(), "Passes");
            embed.AddField(CommentScore(score, entry.Name, score < 50 ? "passed" : "smashed"), $"({score}%)");
            embed.WithImageUrl(entry.Url);
            embed.WithColor(score > 50 ? new Color(0, 165, 249) : new Color(212, 12, 00));

            await Context.Channel.SendMessageAsync("", false, embed);

            SmashDatabase.RemoveEntry(entry.Id);
            Logger.Log($"Ending SoP on user {entry.Name}", "ConfirmStopSoP", LogSeverity.Info);
        }

        [Command("help")]
        public async Task Help()
        {
            await Context.Channel.SendMessageAsync("```" +
                                                   "!help        Shows help screen\n" +
                                                   "!start       Starts SoP, send your picture by message\n" +
                                                   "             attachment or just use link as argument\n" +
                                                   "!stop        Stops your SoP and displays the score\n" +
                                                   "!pass @usr   Passes selected user\n" +
                                                   "!smash @usr  Smashes selected user\n" +
                                                   "!score @usr  Shows selected user score\n" +
                                                   "!listall     Lists all currently running SoPs```");
        }

        private bool IsImage(string url)
        {
            switch (MimeTypes.GetMimeType(url))
            {
                case "image/png":
                case "image/jpeg":
                case "image/bmp":
                case "image/gif":
                    return true;
                default:
                    return false;
            }
        }

        private string CommentScore(short score, string name, string smashOrPass)
        {
            score = (short)Math.Abs((decimal)(score - 50));

            switch (score)
            {
                case short n when (n >= 45):
                    return $"{name} is {smashOrPass} by everyone!";
                case short n when (n >= 30 && n < 45):
                    return $"{name} is almost all the {smashOrPass}!";
                case short n when (n >= 20 && n < 30):
                    return $"{name} is mostly {smashOrPass}!";
                case short n when (n >= 10 && n < 20):
                    return $"{name} is usually {smashOrPass}";
                case short n when (n < 10):
                    return $"{name} voters have mixed opinion";
            }

            return "Well... something bad happened";
        }
    }
}
