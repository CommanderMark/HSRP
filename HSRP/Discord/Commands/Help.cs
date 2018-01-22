using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;
using Discord.Commands;

namespace HSRP.Commands
{
    [Group("help")]
    public class HelpCommands : JModuleBase
    {
        [Command]
        public async Task Help()
        {
            await ReplyAsync("This is the HSRP bot. For use on "
                + "Akumado's RP server.");
        }

        [Command("blood")]
        public async Task HelpBlood()
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

        [Command("ailments"), Alias("ailment", "status effects")]
        public async Task AilmentHelp()
        {
            string msg = Syntax.ToBold("List of ailments") + ":\n";

            foreach (KeyValuePair<string, StatusEffect> sa in Toolbox.StatusEffects)
            {
                msg += "\n-" + sa.Value.Name;
            }

            await ReplyAsync(msg);
        }

        [Command("ailments"), Alias("ailment", "status effects")]
        public async Task AilmentHelp([Remainder] string ailName)
        {
            foreach (KeyValuePair<string, StatusEffect> sa in Toolbox.StatusEffects)
            {
                if (sa.Value.Name.StartsWith(ailName, true, CultureInfo.InvariantCulture))
                {
                    await ReplyAsync(sa.Value.Display());
                    return;
                }
            }

            await ReplyAsync("Ailment not found. If you were trying to find an ailment that is attached to a "
                + "specific move then it will not show up using this command.");
        }

        [Command("skills")]
        public async Task HelpSkills()
        {
            await ReplyAsync(Skills());
        }

        [Command("skills")]
        public async Task HelpSkills(PropertyInfo prop)
        {
            AbilityAttribute attrib = (AbilityAttribute)prop.GetCustomAttribute(typeof(AbilityAttribute), false)
                ?? new AbilityAttribute(AbilityType.None, false, "Errored");
            string ab = Syntax.ToCodeLine($"{prop.Name} ({attrib.ToString()})") + "\n";
            ab = ab.AddLine(attrib.Desc);
            await DiscordToolbox.DMUser(Context.User, ab);
        }

        public static string Skills()
        {
            string msg = "List of Base Abilities:";
            foreach (PropertyInfo prop in typeof(AbilitySet).GetProperties())
            {
                AbilityAttribute attrib = (AbilityAttribute)prop.GetCustomAttribute(typeof(AbilityAttribute), false)
                    ?? new AbilityAttribute(AbilityType.None, false, "Errored");
                msg += "\n" + Syntax.ToCodeLine($"{prop.Name} ({attrib.ToString()})");
            }

            msg += $"\n\nType `{Constants.BotPrefix}help skills [ability name]` for information on each skill.";
            msg += $"\nType `{Constants.BotPrefix}skills spend [ability name] [amount]` to invest pending skill points "
            + "you may have into a specific stat. Remember that spent skill points can not be refunded.";

            return msg;
        }
    }
}