using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace HSRP.Commands
{
    public class StatsCommands : JModuleBase
    {
        [Command("inflict damage"), RequireGM]
        public async Task Inflict(Player plyr, int amount)
        {
            bool before = plyr.Health <= 0;
            bool after = plyr.InflictDamage(amount);
            if (!before && after)
            {
                await ReplyAsync($"{amount} damage was inflicted on {Syntax.ToCodeLine(plyr.Name)}."
                    + $"\nThey are now KO'd at {plyr.Health}/{plyr.MaxHealth} hitpoints.");
            }
            else if (before && !after)
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

        [Command("skills"), RequireRegistration]
        public async Task Skills() => await ReplyAsync(HelpCommands.Skills());

        [Command("skills spend"), RequireRegistration]
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
            
            if (ability.CanRead && ability.CanWrite)
            {
                int value = (int)ability.GetValue(plyr.BaseAbilities);
                ability.SetValue(plyr.BaseAbilities, value + amount);

                plyr.PendingSkillPointAllocations -= amount;

                string msg = $"You have added {amount} skill point(s) into {ability.Name}."
                    + $" ({value} -> {value + amount})";
                if (plyr.PendingSkillPointAllocations >= 1)
                {
                    msg += "\n\nYou have " + plyr.PendingSkillPointAllocations
                        + " skill point(s) left to allocate.";
                }

                plyr.Save();
                await ReplyAsync(msg);
            }
            else
            {
                await ReplyAsync("Something went wrong, go tell Mark.");
            }
        }

        [Command("xp award"), Alias("xp give", "xp"), RequireGM]
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

                IGuildUser user = await plyr.GuildUser;
                await DiscordToolbox.DMUser(user, msg + $"\nYou now have {plyr.PendingSkillPointAllocations} skill points to spend!"
                    + $"\nYour maximum health has been increased to {plyr.MaxHealth}.");
            }

            plyr.Save();
        }
    }
}