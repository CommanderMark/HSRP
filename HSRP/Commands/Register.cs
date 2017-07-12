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
                // If a phase is succesful, then the number
                // is incremented by the time we get here.
                // Otherwise, the number remains the same.
                // Registering/Name.
                case 1:
                    msg = "You are now in the process of registering."
                        + "\nThis is a multi-step process, you will only be registered once this process is complete."
                        + $"\n\nStart by typing `{Constants.BotPrefix}register [your character's name]`.";
                    break;
                
                // Blood color.
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
                            + $"\nRefer to `{Constants.BotPrefix}help blood` for valid colors.";
                    }
                    break;

                // Specibus.
                case 3:
                    if (result)
                    {
                        msg = $"Your character's blood color is {plyr.Name}."
                            + "\n\nNext, enter your strife specibus' name."
                            + $"\nType `{Constants.BotPrefix}register [specibus]`.";
                    }
                    break;

                // Lusus description.
                case 4:
                    if (result)
                    {
                        msg = $"Your character's strife specibus is {plyr.Specibus}."
                            + "\n\nNext, enter a brief description of your lusus."
                            + "\nTheir name, physical appearance, ect. Describe it in"
                            + "60 characters or less.";
                    }
                    else
                    {
                        msg = $"Your description was {input.Length} characters long."
                            + "The limit is 60 characters.";
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