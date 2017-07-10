using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.Commands;

namespace HSRP.Commands
{
    [Group("register")]
    public class Register : ModuleBase
    {
        [Command]
        public async Task Registering([Remainder] string input = "")
        {
            if (Player.Registered(Context.User.Id))
            {
                await ReplyAsync(failure[0]);
            }
            
            Player plyr = new Player(Context.User);
            if (!Program.Instance.Registers.ContainsKey(Context.User.Id))
            {
                 plyr.ID = Context.User.Id;
            }

            if (plyr.Register(input))
            {
                await ReplyAsync(success[Program.Instance.Registers[Context.User.Id]]);
                plyr.Save();
            }
            else
            {
                await ReplyAsync(failure[Program.Instance.Registers[Context.User.Id]]);
            }
        }

        private static List<string> success = new List<string>
        {
            "If you see this message then something is wrong with the bot. Report it! Error code: 0s",

            "You are now in the process of registering."
            + "\nThis is a multi-step process, you will only be registered once this process is complete."
            + $"\n\nStart by typing `{Constants.BotPrefix}register [your character's name]`.",

            "What is your character's blood color?"
            + $"\nType `{Constants.BotPrefix}register [blood color]`."
            + $"\nRefer to `{Constants.BotPrefix}help blood` for colors.",


        };

        private static List<string> failure = new List<string>
        {
            "You are already registered.",

            "If you see this message then something is wrong with the bot. Report it! Error code: 1f",

            "If you see this message then something is wrong with the bot. Report it! Error code: 2f",
        };
    }
}