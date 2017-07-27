using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Discord.Commands;

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

                foreach (string dir in dirs)
                {
                    if (dir.Contains("DS_Store")) { continue; }
                    
                    Player plyr = new Player(dir);
                    if (plyr.Errored)
                    {
                        err = err.AddLine(dir);
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