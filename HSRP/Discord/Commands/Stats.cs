using System.Threading.Tasks;
using Discord.Commands;

namespace HSRP.Commands
{
    [RequireGM]
    public class StatsCommands : JModuleBase
    {
        [Command("damage"), Alias("inflict")]
        public async Task Inflict(Player plyr, int amount)
        {
            bool val = plyr.Health > 0;
            if (plyr.InflictDamage(amount) && val)
            {
                await ReplyAsync($"{amount} damage was inflicted on {Syntax.ToCodeLine(plyr.Name)}."
                    + $"\nThey are now KO'd at {plyr.Health}/{plyr.MaxHealth} hitpoints.");
            }
            else if (!(plyr.InflictDamage(amount) || val))
            {
                await ReplyAsync($"{amount} damage was inflicted on {Syntax.ToCodeLine(plyr.Name)}."
                   + $"\nThey are no longer KO'd at {plyr.Health}/{plyr.MaxHealth} hitpoints.");
            }
            else
            {
                await ReplyAsync($"{amount} damage was inflicted on {Syntax.ToCodeLine(plyr.Name)}."
                    + $"\nThey are now at {plyr.Health}/{plyr.MaxHealth} hitpoints.");
            }
            
            plyr.Save();
        }
    }
}