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
                            + "60 characters or less."
                            + $"\nType `{Constants.BotPrefix}register [lusus description]`.";
                    }
                    else
                    {
                        msg = $"Your description was {input.Length} characters long."
                            + "The limit is 60 characters.";
                    }
                    break;
                
                // Pineapple.
                case 5:
                    if (result)
                    {
                        msg = "\n\nFinal question. You do you like pineapple on pizza?"
                            + $"\nType `{Constants.BotPrefix}register [yes/no]`.";
                    }
                    else
                    {
                        msg = "Invalid answer.";
                    }
                    break;

                // Done.
                case 6:
                    if (result)
                    {
                        msg = "Alright, you are now registered!";
                    }
                    break;
            }
            await DiscordToolbox.DMUser(Context.User, msg);

            if (Program.Instance.Registers[Context.User.Id] <= 6)
            {
                Program.Instance.Registers.Remove(Context.User.Id);
                plyr.PendingSkillPointAllocations = 24;
                await Info();
            }
            plyr.Save();
        }
        
        private async Task Info()
        {
            string message = "Some other stuff that's important to know follows."
                + "\n\nYour base stats are divided into 3 categories. Physical, Mental and Speech. "
                + " Each category has 2 stats, one for offensive capability, the other for defensive."
                + $"\n\n`{Constants.BotPrefix}"
        }
    }
}