using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace HSRP.Commands
{
    // TODO: XP System
    [RequireRegistration]
    public class LevelUp : ModuleBase
    {
        [Command("allocate"), Alias("spend")]
        public async Task SpendSkill(PropertyInfo ability, int amount)
        {
            Player plyr = new Player(Context.User);
            if (plyr.PendingSkillPointAllocations < 1)
            {
                await ReplyAsync("You have no pending skill point allocations.");
                return;
            }
            else if (plyr.PendingSkillPointAllocations < amount)
            {
                await ReplyAsync("You are attempting to allocate more skill points than you actually have."
                    + $"\nYou have {plyr.PendingSkillPointAllocations}, you were trying to spend {amount}.");
                return;
            }
            
            int value = (int)ability.GetValue(plyr.Abilities);
            ability.SetValue(plyr.Abilities, value + amount);

            plyr.PendingSkillPointAllocations -= amount;

            string msg = $"You have added {amount} skill point(s) into {ability.Name}."
                + $" ({value} -> {value + amount})";
            if (plyr.PendingSkillPointAllocations >= 1)
            {
                msg += "\n\nYou have " + plyr.PendingSkillPointAllocations
                    + " skill points left to allocate.";
            }

            plyr.Save();
            await ReplyAsync(msg);
        }

        [Command("givexp"), Alias("xp", "award"), RequireGM]
        public async Task GrantXP(Player plyr, int amount)
        {
            if (amount <= 0)
            {
                await ReplyAsync("Invalid XP amount.");
                return;
            }

            int levels = plyr.GiveXP(amount);
            await ReplyAsync($"{Syntax.ToCodeLine(plyr.Name)} has been awarded {amount} XP.");
            if (levels > 0)
            {
                string count = levels > 1
                    ? levels + " levels"
                    : "a level";
                
                string msg = $"{Syntax.ToCodeLine(plyr.Name)} has gained {count}!";

                await ReplyAsync(msg);
                await DiscordToolbox.DMUser(Context.User, msg + $"\nYou now have {plyr.PendingSkillPointAllocations} skill points to spend!"
                    + $"\nYour maximum health has been increased to {plyr.MaxHealth}.");
            }

            plyr.Save();
        }
    }
}
