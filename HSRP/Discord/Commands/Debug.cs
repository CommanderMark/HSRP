using System.IO;
using System.Linq;
using System.Threading.Tasks;
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

        [Command("shit")]
        public async Task Shit([Remainder]string text)
        {
            string[] words = text.ToLower().Split(' ');

            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length == 1 || words[i].Length == 2)
                {
                    words[i] = words[i].ToUpper();
                    continue;
                }

                words[i] = words[i].Substring(0, 1).ToUpper()
                    + words[i].Substring(1, words[i].Length - 2).ToLower()
                    + words[i].Substring(words[i].Length - 1, 1).ToUpper();
            }

            await ReplyAsync(string.Join(" ", words));
        }
    }
}