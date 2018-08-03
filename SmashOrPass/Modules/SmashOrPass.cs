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
                    Console.WriteLine($"[Error][StartSoP] No photo attached!");
                    await Context.Channel.SendMessageAsync("Send me your photo to start!");
                    return;
                }
            }

            if (!IsImage(url))
            {
                Console.WriteLine($"[Error][StartSoP] {MimeTypes.GetMimeType(url)} is not a photo!");
                await Context.Channel.SendMessageAsync("Send me your photo to start!");
                return;
            }

            var embed = new EmbedBuilder();
            embed.WithTitle("Smash Or Pass");
            embed.WithDescription($"{Context.User.Username} has started Smash Or Pass!"); //
            embed.AddInlineField($"!smash @{Context.User.Username}", "Vote to smash");
            embed.AddInlineField($"!pass @{Context.User.Username}", "Vote to pass");
            embed.WithImageUrl(url);
            embed.WithColor(new Color(255, 22, 148));

            await Context.Channel.SendMessageAsync("", false, embed);

            Console.WriteLine($"[Success][StartSoP] Starting SoP on user {Context.User.Username}");
            SmashDatabase.AddEntry(new UserEntry(){Id = Context.User.Id, Url = url, Name = Context.User.Username});
        }

        [Command("pass")]
        public async Task Pass([Optional]string target)
        {
            if (string.IsNullOrEmpty(target))
            {
                Console.WriteLine($"[Error][PassSoP] No user specified");
                await Context.Channel.SendMessageAsync("Tell me who you want to pass by writing !pass {@user}");
                return;
            }

            ulong id = Convert.ToUInt64(target.Substring(2, target.Length - 3));
            UserEntry entry = null;

            if (!SmashDatabase.HasUser(id))
            {
                Console.WriteLine($"[Error][PassSoP] User not in database!");
                await Context.Channel.SendMessageAsync("This user did not start his Smash Or Pass!");
                return;
            }

            entry = SmashDatabase.GetEntry(id);

            if (entry.RatedBy.Contains(Context.User.Id))
            {
                Console.WriteLine($"[Error][PassSoP] User has already voted!");
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
            embed.WithColor(new Color(255, 22, 148));

            await Context.Channel.SendMessageAsync("", false, embed);

            Console.WriteLine($"[Success][PassSoP] User {Context.User.Username} passed {entry.Name}");
        }

        [Command("smash")]
        public async Task Smash([Optional]string target)
        {
            if (string.IsNullOrEmpty(target))
            {
                Console.WriteLine($"[Error][PassSoP] No user specified");
                await Context.Channel.SendMessageAsync("Tell me who you want to smash by writing !smash {@user}");
                return;
            }

            ulong id = Convert.ToUInt64(target.Substring(2, target.Length - 3));
            UserEntry entry = null;

            if (!SmashDatabase.HasUser(id))
            {
                Console.WriteLine($"[Error][PassSoP] User not in database!");
                await Context.Channel.SendMessageAsync("This user did not start his Smash Or Pass!");
                return;
            }

            entry = SmashDatabase.GetEntry(id);

            if (entry.RatedBy.Contains(Context.User.Id))
            {
                Console.WriteLine($"[Error][PassSoP] User has already voted!");
                await Context.Channel.SendMessageAsync($"You have already rated {entry.Name}. To see current results write: !score @{entry.Name}");
                return;
            }

            entry.Smashes++;
            entry.RatedBy.Add(Context.User.Id);
            SmashDatabase.UpdateEntry(entry);

            var embed = new EmbedBuilder();
            embed.WithTitle("Smash Or Pass");
            embed.WithDescription($"{Context.User.Username} smashed {entry.Name}!");
            embed.AddInlineField(entry.Smashes.ToString(), "Total Smashes");
            embed.AddInlineField(entry.Passes.ToString(), "Total Passes");
            embed.WithImageUrl(entry.Url);
            embed.WithColor(new Color(255, 22, 148));

            await Context.Channel.SendMessageAsync("", false, embed);

            Console.WriteLine($"[Success][PassSoP] User {Context.User.Username} smashed {entry.Name}");
        }

        [Command("score")]
        public async Task Score([Optional]string target)
        {
            if (string.IsNullOrEmpty(target))
            {
                Console.WriteLine($"[Error][Info] No user specified");
                await Context.Channel.SendMessageAsync("Tell me whose score you want to see by writing !score {@user}");
                return;
            }

            ulong id = Convert.ToUInt64(target.Substring(2, target.Length - 3));
            UserEntry entry = null;

            if (!SmashDatabase.HasUser(id))
            {
                Console.WriteLine($"[Error][Info] User not in database!");
                await Context.Channel.SendMessageAsync("This user did not start his Smash Or Pass!");
                return;
            }

            entry = SmashDatabase.GetEntry(id);

            var embed = new EmbedBuilder();
            embed.WithTitle("Smash Or Pass");
            embed.WithDescription($"{Context.User.Username} score");
            embed.AddInlineField(entry.Smashes.ToString(), "Smashes");
            embed.AddInlineField(entry.Passes.ToString(), "Passes");
            embed.WithImageUrl(entry.Url);
            embed.WithColor(new Color(255, 22, 148));

            await Context.Channel.SendMessageAsync("", false, embed);

            Console.WriteLine($"[Success][ScoreSoP] {entry.Name} ");
        }

        [Command("stop")]
        public async Task Stop()
        {
            ulong id = Context.User.Id;
            UserEntry entry = null;

            if (!SmashDatabase.HasUser(id))
            {
                Console.WriteLine($"[Error][PassSoP] User not in database!");
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
            embed.WithColor(new Color(255, 22, 148));

            await Context.Channel.SendMessageAsync("", false, embed);

            SmashDatabase.RemoveEntry(entry.Id);
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
                                                   "!score @usr  Shows selected user score```");
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
