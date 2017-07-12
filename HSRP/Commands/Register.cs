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
                await ReplyAsync("You are already registered.");
                return;
            }
            
            Player plyr = new Player(Context.User);
            if (!Program.Instance.Registers.ContainsKey(Context.User.Id))
            {
                 plyr.ID = Context.User.Id;
            }

            bool result = plyr.Register(input);
            string msg = "";

            switch (Program.Instance.Registers[Context.User.Id])
            {
                // Registering/Name
                case 1:
                    msg = "You are now in the process of registering."
                        + "\nThis is a multi-step process, you will only be registered once this process is complete."
                        + $"\n\nStart by typing `{Constants.BotPrefix}register [your character's name]`.";
                    break;
                
                // Blood Color
                case 2:
                    if (result)
                    {
                        msg = $"Your character's name is {plyr.Name}."
                            + "\n\nWhat is your character's blood color?"
                            + $"\nType `{Constants.BotPrefix}register [blood color]`."
                            + $"\nRefer to `{Constants.BotPrefix}help blood` for colors.";
                    }
                    else
                    {
                        msg = "Invalid blood color."
                            + $"\nRefer to `{Constants.BotPrefix}help blood` for colors.";
                    }
                    break;

                // Specibus.
                case 3:
                    if (result)
                    {
                        msg = $"Your character's blood color is {plyr.Name}."
                            + "\n\nWhat is your character's blood color?";
                    }
                    break;
            }

            await DiscordToolbox.DMUser(Context.User, msg);
        }

        private static List<string> success = new List<string>
        {

            "What is your character's blood color?"
            + $"\nType `{Constants.BotPrefix}register [blood color]`."
            + $"\nRefer to `{Constants.BotPrefix}help blood` for colors.",


        };
    }
}