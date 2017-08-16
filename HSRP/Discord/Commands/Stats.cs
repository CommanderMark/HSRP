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
            bool val2 = plyr.Health > 0;
            bool val1 = plyr.InflictDamage(amount);
            if (val1 && val2)
            {
                await ReplyAsync($"{amount} damage was inflicted on {Syntax.ToCodeLine(plyr.Name)}."
                    + $"\nThey are now KO'd at {plyr.Health}/{plyr.MaxHealth} hitpoints.");
            }
            else if (!(val1 || val2))
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