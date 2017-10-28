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