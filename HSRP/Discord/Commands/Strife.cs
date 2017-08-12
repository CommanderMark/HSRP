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

            bool attackAtks = false;
            if ("attackers".StartsWith(who, StringComparison.OrdinalIgnoreCase)
                || who.Equals("atk", StringComparison.OrdinalIgnoreCase))
            {
                attackAtks = true;
            }
            else if ("targets".StartsWith(who, StringComparison.OrdinalIgnoreCase))
            {
                attackAtks = false;
            }
            else
            {
                await ReplyAsync("Invalid input.");
                return;
            }

            if (strf.CurrentTurner.ID == plyr.ID)
            {
                if (strf.ValidateTurn(sa, index, attackAtks, out string reason))
                {
                    await ReplyStrifeAsync(strf.TakeTurn(sa, index, attackAtks));
                    await ReplyStrifeSegmentAsync(strf.UpdateStrife(out Player next));
                    strf.Save();
                }
                else
                {
                    await ReplyAsync(reason);
                }
            }
            else
            {
                await ReplyAsync($"It is {Syntax.ToCodeLine(strf.CurrentTurner.Name.ToApostrophe())} turn.");
            }
        }

        [Command("turn"), InStrife]
        public async Task Turn()
        {
            Strife strf = Context.GetStrife();
            await ReplyAsync(strf.UpdateStrife(out Player next));
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
        public async Task Check() => await Check(Context.GetPlayerEntity().StrifeID);

        [Command("check"), RequireGM]
        public async Task Check(int id)
        {
            Strife strf = new Strife(id.ToString());
            await ReplyAsync(Syntax.ToCodeBlock(strf.Display()));
        }

        [Command("identify"), InStrife]
        public async Task Identify(string who, int index) => await Identify(Context.GetPlayerEntity().StrifeID, who, index);

        [Command("identify"), RequireGM]
        public async Task Identify(int id, string who, int index)
        {
            Strife strf = new Strife(id.ToString());

            bool attackAtks = false;
            if ("attackers".StartsWith(who, StringComparison.OrdinalIgnoreCase)
                || who.Equals("atk", StringComparison.OrdinalIgnoreCase))
            {
                attackAtks = true;
            }
            else if ("targets".StartsWith(who, StringComparison.OrdinalIgnoreCase))
            {
                attackAtks = false;
            }
            else
            {
                await ReplyAsync("Invalid input.");
                return;
            }

            IEntity ent = strf.GetTarget(index, attackAtks);
            if (ent == null)
            {
                await ReplyAsync("Invalid strifer.");
            }
            else
            {
                await ReplyAsync(Syntax.ToCodeBlock(ent.Display(true)));
            }
        }

        [Command("log"), InStrife]
        public async Task Log() => await Log(Context.GetPlayerEntity().StrifeID);

        [Command("log"), RequireGM]
        public async Task Log(int id)
        {
            Strife strf = new Strife(id.ToString());
            string txt = string.Join("\n", strf.Logs);
            string path = strf.LogLogs();

            await Context.Channel.SendFileAsync(path);
            File.Delete(path);
        }

        [Command("clear"), RequireGM]
        public async Task ClearLogs(int id)
        {
            Strife strf = new Strife(id.ToString());
            string path = strf.ClearLogs();

            await Context.Channel.SendFileAsync(path, "Logs cleared.");
            File.Delete(path);
        }
    }
}