using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Discord.Commands;
using Discord;
using System.Xml.Linq;
using System.Collections.Generic;

namespace HSRP.Commands
{
    [Group("debug"), RequireGM]
    public class DebugCommands : JModuleBase
    {
        [Command("filter")]
        public async Task FilterXml(string type)
        {
            if (type == "users")
            {
                string err = "";
                string[] dirs = Directory.GetFiles(Dirs.Players);

                foreach (string file in dirs)
                {
                    if (file.Contains("DS_Store")) { continue; }
                    
                    Player plyr = new Player(file, false);
                    if (plyr.Errored)
                    {
                        err = err.AddLine(file);
                        continue;
                    }

                    plyr.Save();
                }

                if (string.IsNullOrWhiteSpace(err))
                {
                    await ReplyAsync("Done.");
                }
                else
                {
                    await ReplyAsync("Done, these files failed: " + err);
                }
            }
            else if (type == "heal")
            {
                string[] dirs = Directory.GetFiles(Dirs.Players);
                foreach (string file in dirs)
                {
                    if (file.Contains("DS_Store")) { continue; }
                    
                    Player plyr = new Player(file, false);
                    if (plyr.Errored) { continue; }

                    plyr.Health = plyr.MaxHealth;
                    plyr.Save();
                }

                await ReplyAsync("All players healed.");
            }
        }

        [Command("strifelist")]
        public async Task Strifes()
        {

            string txt = "";
            string[] dirs = Directory.GetFiles(Dirs.Strifes);

            foreach (string file in dirs)
            {
                Strife strf = new Strife(file, false);
                if (strf.Active)
                {
                    txt = txt.AddLine(strf.ID.ToString());
                }
            }

            await ReplyAsync(string.IsNullOrWhiteSpace(txt)
                ? "There are currently no active strifes."
                : txt);
        }

        [Command("togglestrife"), Alias("strifetoggle")]
        public async Task ToggleStrifeChannel()
        {
            if (Constants.STRIFE_CHANNEL == Constants.RP_STRIFE_CHANNEL)
            {
                Constants.STRIFE_CHANNEL = Constants.TEST_STRIFE_CHANNEL;
                await ReplyAsync("Strife channel is set to the TEST one.");
            }
            else
            {
                Constants.STRIFE_CHANNEL = Constants.RP_STRIFE_CHANNEL;
                await ReplyAsync("Strife channel is set to the RP one.");
            }
        }

        [Command("npcs"), Alias("npc")]
        public async Task ListNPCs()
        {
            SortedDictionary<ulong, string> ents = new SortedDictionary<ulong, string>();
            string[] dirs = Directory.GetFiles(Dirs.NPCs);

            foreach (string file in dirs)
            {
                if (file.Contains("DS_Store")) { continue; }
                
                if (NPC.TryParse(file, out NPC npc, false))
                {
                    ents.Add(npc.ID, npc.Name);
                }
            }

            string msg = "";
            foreach (KeyValuePair<ulong, string> npc in ents)
            {
                msg = msg.AddLine($"{npc.Key} - {npc.Value}");
            }

            await ReplyAsync(Syntax.ToCodeBlock(msg));
        }

        [Command("npcs"), Alias("npc")]
        public async Task GetNPC(int id)
        {
            if (NPC.TryParse(id.ToString(), out NPC npc))
            {
                await ReplyAsync(Syntax.ToCodeBlock(npc.Display()));
            }
            else
            {
                await ReplyAsync("NPC not found.");
            }
        }

        // OMV Quirk Stuff
        private string ToQuirk(string str)
        {
            str = Regex.Replace(str, @"\b(\w)", m => m.ToString().ToUpper());
            str = Regex.Replace(str, @"(\w)\b", m => m.ToString().ToUpper());
            return str;
        }
        
        [Command("quirk")]
        public async Task Quirk([Remainder] string text)
        {
            await ReplyAsync(ToQuirk(text));
        }

        [Command("pester")]
        public async Task Pester([Remainder] string text)
        {
            await ReplyAsync("\\`OMV\\`: " + ToQuirk(text));
        }
    }
}