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
        [Command("levelup")]
        public async Task Level()
        {
            Player plyr = new Player(Context.User);
            if (plyr.PendingLevelUps < 1)
            {
                await ReplyAsync("You have no pending level ups.");
                return;
            }

            bool result = plyr.LevelUp();
            string msg = $"You are now at rung {plyr.Echeladder} of your Echeladder!"
                + $"\n Your maximum health has been increased to {plyr.Health}.";
            if (result)
            {
                msg += "\n\nYou've gained an additional skill point that you can spend on"
                + $" one of your base skills. Use `{Constants.BotPrefix}spendskill [skill name]`"
                + " to spend the skill point.";
            }

            await ReplyAsync(msg);
        }

        [Command("spendskill")]
        public async Task SpendSkill(BaseAbility ability)
        {
            Player plyr = new Player(Context.User);
            if (plyr.PendingSkillPointAllocations < 1)
            {
                await ReplyAsync("You have no pending skill point allocations.");
                return;
            }

            PropertyInfo prop = ability.GetAbilityProperty();
            int value = (int)prop.GetValue(plyr.Abilities);
            prop.SetValue(plyr.Abilities, value + 1);

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
