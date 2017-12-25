using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Discord.Commands;
using Discord;
using System.Collections.Generic;

namespace HSRP.Commands
{
    [Group("debug"), RequireGM]
    public class DebugCommands : JModuleBase
    {
        [Command("filter")]
        public async Task FilterXml(string type)
        {
            if (type == "users" || type == "players")
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

        [Command("npc add")]
        public async Task AddNPC() => await AddNPC(string.Empty);

        [Command("npc add")]
        public async Task AddNPC([Remainder] string str)
        {
            const string PARSER_CHARCTERS = "```";

            int start = str.IndexOf(PARSER_CHARCTERS);
            int end = str.LastIndexOf(PARSER_CHARCTERS);

            if (start < 0)
            {
                await ReplyAsync("Parse characters not found in message. "
                    + "Remember to put \\`\\`\\` before the npc's xml data.");

                return;
            }

            string xml = string.Empty;
            start += 3;

            /// Message is "```..." somehow.
            if (start == end)
            {
                xml = str.Substring(start, str.Length - 1 - start);
            }
            /// Message is "```...```"
            else
            {
                xml = str.Substring(start, end - start);
            }

            // Parse it to xml.
            try
            {
                XDocument doc = XDocument.Parse(xml, LoadOptions.PreserveWhitespace);
                
                ulong id = XmlToolbox.GetAttributeUnsignedLong(doc.Root, "id", 0);
                if (id <= 0)
                {
                    await ReplyAsync("NPC did not have a valid ID.");
                    return;
                }

                string npcDir = Dirs.NPCs + "/" + id + ".xml";

                if (File.Exists(npcDir))
                {
                    await ReplyAsync("NPC " + id + " already exists.");
                }
                else
                {
                    XmlToolbox.WriteXml(npcDir, doc);
                    
                    // Display NPC
                    if (NPC.TryParse(id.ToString(), out NPC npc))
                    {
                        await ReplyAsync(Syntax.ToCodeBlock(npc.Display()));
                    }
                    else
                    {
                        await ReplyAsync("NPC was saved but appears to be corrupted.");
                    }
                }
            } 
            catch
            {
                await ReplyAsync("NPC could not be parsed.");
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