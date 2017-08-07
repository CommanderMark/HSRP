using Discord.Commands;
using System.Threading.Tasks;
using Discord;

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

    }
}