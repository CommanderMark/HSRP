using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Discord.Commands;
using Discord;

namespace HSRP.Commands
{
    public class DebugCommands : JModuleBase
    {
        [RequireJorge]
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
                    
                    Player plyr = new Player(file);
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
        }

        [Command("strifelist"), RequireGM]
        public async Task Strifes()
        {
            string txt = "";
            foreach (int i in Strife.ActiveStrifes)
            {
                txt = txt.AddLine(i.ToString());
            }

            await ReplyAsync(txt);
        }

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