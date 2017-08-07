using Discord.Commands;
using System.Threading.Tasks;
using Discord;
using System.IO;

namespace HSRP.Commands
{
    [Group("strife")]
    public class StrifeCommands : JModuleBase
    {
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
                await ReplyAsync("Strife activated!");

                await ReplyStrifeAsync(await strf.ActivateStrife());
                strf.Save();
            }
            else
            {
                await ReplyAsync("Strife not found.");
            }
        }

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