using Discord.Commands;
using System.Threading.Tasks;
using System.IO;
using System;
using System.Linq;
using System.Collections.Generic;

namespace HSRP.Commands
{
    [Group("strife")]
    public class StrifeCommands : JModuleBase
    {
        /// <summary>
        /// Continually updates the strife until a human strifer is next.
        /// </summary>
        private async Task UpdateStrifeUntilHumanAsync(Strife strf)
        {
            // Keep updating the strife until a human is next.
            bool humanNext = false;
            string msg = string.Empty;
            while (!humanNext)
            {
                // TODO: Add delay?
                if (msg.Length >= Constants.DiscordCharLimit)
                {
                    await ReplyStrifeAsync(msg);
                    msg = string.Empty;
                }

                Tuple<string, bool> tup = strf.UpdateStrife();
                msg += "\n" + tup.Item1;
                humanNext = tup.Item2;
            }
        }

        [Command("action"), InStrife]
        public async Task Action(string who, int index, string action)
        {
            // Figure out whether this is a pre-defined strife action or a move.
            // TODO: Moves.
            IEnumerable<StrifeAction> fields = Enum.GetValues(typeof(StrifeAction)).Cast<StrifeAction>();
            foreach (StrifeAction sa in fields)
            {
                if (sa.ToString().StartsWith(action, StringComparison.OrdinalIgnoreCase))
                {
                    action = sa.ToString();
                    break;
                }
            }

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

            if (strf.ValidateTurn(action, index, attackAtks, plyr, out string reason))
            {
                await ReplyStrifeAsync(strf.TakeTurn(action, index, attackAtks));

                // Keep updating the strife until a human is next.
                await UpdateStrifeUntilHumanAsync(strf);

                // If the strife is no longer active then it was completed this turn. So post logs.
                if (!strf.Active)
                {
                    string path = strf.LogLogs();

                    await Context.Channel.SendFileAsync(path, "The log of the strife is now being posted.");
                    File.Delete(path);
                }

                strf.Save();
            }
            else
            {
                await ReplyAsync(reason);
            }
        }

        [Command("turn"), InStrife]
        public async Task Turn()
        {
            Strife strf = Context.GetStrife();
            await ReplyAsync(strf.UpdateStrife().Item1);
        }

        [Command("forfeit"), InStrife]
        public async Task Forfeit()
        {
            Strife strf = Context.GetStrife();
            await ReplyStrifeAsync(strf.Forfeit(Context.User.Id));
            await ReplyStrifeAsync(strf.UpdateStrife().Item1);
            // If the strife is no longer active then it was completed this turn. So post logs.
            if (!strf.Active)
            {
                string path = strf.LogLogs();

                await Context.Channel.SendFileAsync(path, "The log of the strife is now being posted.");
                File.Delete(path);
            }

            strf.Save();
        }

        [Command("generate"), Alias("create"), RequireGM]
        public async Task Generate(int id = 0)
        {
            if (Strife.TryCreateStrife(id, out Strife strf))
            {
                strf.Save();
                await ReplyAsync("Strife " + strf.ID + " has been created.");
            }
            else
            {
                await ReplyAsync("A strife with that ID already exists.");
            }
        }

        [Command("delete"), RequireGM]
        public async Task Delete(int id)
        {
            string filePath = Path.Combine(Dirs.Strifes, id + ".xml");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                await ReplyAsync("Strife " + id + " deleted.");
            }
            else
            {
                await ReplyAsync("Strife not found.");
            }
        }

        [Group("edit"), RequireGM]
        public class EditStrife : JModuleBase
        {
            [Command("atk"), Alias("attack", "at")]
            public async Task Attack(Strife strf, params Entity[] entities)
            {
                foreach (Entity ent in entities)
                {
                    strf.Attackers.Add(ent);
                }

                strf.Save();
                await ReplyAsync(Syntax.ToCodeBlock(strf.Display()));
            }

            [Command("target"), Alias("tar", "targets")]
            public async Task Target(Strife strf, params Entity[] entities)
            {
                foreach (Entity ent in entities)
                {
                    strf.Targets.Add(ent);
                }

                strf.Save();
                await ReplyAsync(Syntax.ToCodeBlock(strf.Display()));
            }

            [Command("clear")]
            public async Task Clear(Strife strf)
            {
                strf.Attackers.Clear();
                strf.Targets.Clear();

                strf.Save();
                await ReplyAsync("Strife's entities cleared.");
            }
        }

        [Command("activate"), Alias("active"), RequireGM]
        public async Task Activate(Strife strf)
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

            await UpdateStrifeUntilHumanAsync(strf);
            strf.Save();
        }

        [Command("Deactivate"), Alias("deactive"), RequireGM]
        public async Task Deactivate(Strife strf)
        {
            await ReplyStrifeAsync(strf.DeactivateStrife());

            // Post logs.
            string path = strf.LogLogs();

            await Context.Channel.SendFileAsync(path, "The log of the strife is now being posted.");
            File.Delete(path);

            strf.Save();
        }

        [Command("end"), RequireGM]
        public async Task End(Strife strf)
        {
            await ReplyAsync(strf.DeactivateStrife());
            strf.Save();
        }

        [Command("check"), Alias("status"), InStrife]
        public async Task Check() => await Check(Context.GetStrife());

        [Command("check"), Alias("status"), RequireGM]
        public async Task Check(Strife strf)
        {
            await ReplyAsync(Syntax.ToCodeBlock(strf.Display()));
        }

        [Command("identify"), Alias("identity"), InStrife, Priority(1)]
        public async Task Identify(string who, int index) => await Identify(Context.GetStrife(), who, index);

        [Command("identify"), Alias("identity"), RequireGM, Priority(0)]
        public async Task Identify(Strife strf, string who, int index)
        {
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

            Entity ent = strf.GetTarget(index, attackAtks);
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
        public async Task Log() => await Log(Context.GetStrife());

        [Command("log")]
        public async Task Log(Strife strf)
        {
            if (strf.Logs.Count < 1)
            {
                await ReplyAsync("There are no logs in this strife.");
                return;
            }
            string path = strf.LogLogs();

            await Context.Channel.SendFileAsync(path);
            File.Delete(path);
        }

        [Command("clearlogs"), RequireGM]
        public async Task ClearLogs(Strife strf)
        {
            if (strf.Logs.Count < 1)
            {
                await ReplyAsync("There are no logs in this strife.");
                return;
            }
            string path = strf.ClearAndLogLogs();

            await Context.Channel.SendFileAsync(path, "Logs cleared.");
            File.Delete(path);
            strf.Save();
        }

        [Command("1v1"), RequireGM]
        public async Task OneVeeOne(params Player[] plyrs)
        {
            if (plyrs == null || plyrs.Length < 2) { return; }

            Strife strf;
            Strife.TryCreateStrife(-1, out strf);

            for (int i = 0; i < plyrs.Length; i++)
            {
                if (i % 2 == 0)
                {
                    strf.Attackers.Add(plyrs[i]);
                }
                else
                {
                    strf.Targets.Add(plyrs[i]);
                }
            }
            strf.Save();
            await ReplyAsync("Strife " + strf.ID + " has been created."
                + "\n" + strf.Display());
        }
    }
}
