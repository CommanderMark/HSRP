using Discord.Commands;
using System.Threading.Tasks;
using System.IO;
using System;

namespace HSRP.Commands
{
    [Group("strife")]
    public class StrifeCommands : JModuleBase
    {
        [Command("action"), InStrife]
        public async Task Action(string who, int index, StrifeAction sa)
        {
            Strife strf = Context.GetStrife();
            Player plyr = Context.GetPlayerEntity();

            if ("attackers".StartsWith(who, StringComparison.OrdinalIgnoreCase)
                || who.Equals("atk", StringComparison.OrdinalIgnoreCase))
            {
                if (index >= strf.Attackers.Count)
                {
                    await ReplyAsync("Invalid index.");
                }

                if (strf.CurrentTurner.ID == plyr.ID)
                {
                    await ReplyStrifeAsync(strf.TakeTurn(sa, index, true));
                    await ReplyStrifeAsync(strf.UpdateStrife(out Player next));
                    await ReplyStrifeAsync($"It is {next.Name}'s turn.");
                    strf.Save();
                }
                else
                {
                    await ReplyAsync($"It is {strf.CurrentTurner.Name}'s turn.");
                }
            }
            else if ("targets".StartsWith(who, StringComparison.OrdinalIgnoreCase))
            {
                if (index >= strf.Targets.Count)
                {
                    await ReplyAsync("Invalid index.");
                }

                if (strf.CurrentTurner.ID == plyr.ID)
                {
                    await ReplyStrifeAsync(strf.TakeTurn(sa, index, false));
                    await ReplyStrifeAsync(strf.UpdateStrife(out Player next));
                    await ReplyStrifeAsync($"It is {next.Name}'s turn.");
                    strf.Save();
                }
                else
                {
                    await ReplyAsync($"It is {strf.CurrentTurner.Name}'s turn.");
                }
            }
            else
            {
                await ReplyAsync("Invalid input.");
            }
        }

        [Command("activate"), RequireGM]
        public async Task Activate(int id)
        {
            Strife strf = new Strife(id.ToString());
            if (!strf.Errored)
            {
                if (strf.Active)
                {
                    await ReplyAsync("Strife is already activated.");
                    return;
                }

                await strf.ActivateStrife();
                await ReplyAsync("Strife activated!");

                await ReplyStrifeAsync("A strife has begun.");
                await ReplyStrifeAsync(Syntax.ToCodeBlock(strf.Display()));
                await ReplyStrifeAsync(strf.UpdateStrife(out Player next));
                strf.Save();
            }
            else
            {
                await ReplyAsync("Strife not found.");
            }
        }

        // TODO: Ability to check individual entity stats.
        [Command("check"), InStrife]
        public async Task Check()
        {
            Strife strf = Context.GetStrife();
            await ReplyAsync(Syntax.ToCodeBlock(strf.Display()));
        }

        [Command("check"), RequireGM]
        public async Task Check(int id)
        {
            Strife strf = new Strife(id.ToString());
            await ReplyAsync(Syntax.ToCodeBlock(strf.Display()));
        }

        [Command("log"), InStrife]
        public async Task Log()
        {
            Strife strf = Context.GetStrife();
            string txt = string.Join("\n", strf.Logs);
            string path = Path.Combine(Dirs.Config, $"LOG_{strf.ID}.txt");
            File.WriteAllText(path, txt);

            await Context.Channel.SendFileAsync(path);
            File.Delete(path);
        }

        [Command("log"), RequireGM]
        public async Task Log(int id)
        {
            Strife strf = new Strife(id.ToString());
            string txt = string.Join("\n", strf.Logs);
            string path = Path.Combine(Dirs.Config, $"LOG_{strf.ID}.txt");
            File.WriteAllText(path, txt);

            await Context.Channel.SendFileAsync(path);
            File.Delete(path);
        }
    }
}