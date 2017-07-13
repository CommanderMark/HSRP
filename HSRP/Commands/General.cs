using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace HSRP.Commands
{
    public class General : ModuleBase
    {
        [Command("profile"), Alias("stats", "prof"), RequireRegistration]
        public async Task Profile()
        {
            try
            {
                Player plyr = new Player(Context.User);
                await ReplyAsync(Syntax.ToCodeBlock(plyr.Display(Context.User)));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        [Command("roll")]
        public async Task RollDice(string input)
        {
            int index = input.LastIndexOf('d');
            if (
                int.TryParse(input.Substring(0, index), out int rolls)
                && int.TryParse(input.Substring(index + 1), out int dieType)
            )
            {
                if (rolls < 1 || dieType < 1)
                {
                    await ReplyAsync("Invalid integer for dice roll.");
                    return;
                }

                await ReplyAsync(Toolbox.DiceRoll(rolls, dieType).ToString());
            }
        }
    }
}
