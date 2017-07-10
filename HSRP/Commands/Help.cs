using System.Reflection;
using System.Threading.Tasks;
using Discord.Commands;

namespace HSRP.Commands
{
    [Group("help")]
    public class HelpCommands : ModuleBase
    {
        [Command]
        public async Task Help()
        {
            await ReplyAsync("This is the HSRP bot. For use on "
                + "Akumado's RP server.");
        }

        [Command("blood")]
        public async Task Help1()
        {
            string msg = "List of valid blood types:";

            var flds = typeof(BloodType).GetFields();
            for (int i = 0; i < flds.Length; i++)
            {
                if (i == 0 || i == 1) { continue; }
                msg += "\n" + flds[i].Name;
            }

            await ReplyAsync(msg);
        }
    }
}