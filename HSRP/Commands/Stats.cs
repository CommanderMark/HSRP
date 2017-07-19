using System.Threading.Tasks;
using Discord.Commands;

namespace HSRP.Commands
{
    [RequireGM]
    public class StatsCommands : JModuleBase
    {
        [Command("damage"), Alias("inflict")]
        public async Task InflictDamage(Player plyr, int amount)
        {
            if (amount <= 0)
            {
                await ReplyAsync("Invalid amount.");
                return;
            }

            plyr.InflictDamage(amount);
        }
    }
}