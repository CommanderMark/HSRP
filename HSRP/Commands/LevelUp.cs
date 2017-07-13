using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace HSRP.Commands
{
    // TODO: XP System
    // TODO: Save player data.
    [RequireRegistration]
    public class LevelUp : ModuleBase
    {
        [Command("spendskill")]
        public async Task SpendSkill(PropertyInfo ability)
        {
            Player plyr = new Player(Context.User);
            if (plyr.PendingSkillPointAllocations < 1)
            {
                await ReplyAsync("You have no pending skill point allocations.");
                return;
            }
            
            int value = (int)ability.GetValue(plyr.Abilities);
            ability.SetValue(plyr.Abilities, value + 1);

            plyr.PendingSkillPointAllocations--;

            string msg = "You have added a skill point into " + ability.ToString() + "."
                + $" ({value} -> {value + 1})";
            if (plyr.PendingSkillPointAllocations >= 1)
            {
                msg += "\n\nYou have " + plyr.PendingSkillPointAllocations
                    + " skill points left to allocate.";
            }

            await ReplyAsync(msg);
        }
    }
}
