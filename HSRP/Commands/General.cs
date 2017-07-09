using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HSRP.Commands
{
    public class General : ModuleBase
    {
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
