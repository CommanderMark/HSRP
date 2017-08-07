using Discord.Commands;
using System.Threading.Tasks;

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

            }
        }

    }
}