﻿using Discord.Commands;
using System;
using System.Threading.Tasks;
using Discord;

namespace HSRP.Commands
{
    public class General : JModuleBase
    {
        [RequireRegistration]
        [Command("profile"), Alias("stats", "prof")]
        public async Task Profile() => await Profile(new Player(Context.User));

        [Command("profile"), Alias("stats", "prof")]
        public async Task Profile(Player plyr)
        {
            await ReplyAsync(Syntax.ToCodeBlock(plyr.Display()));
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

        [RequireJorge]
        [Command("stfu"), Alias("kys", "die")]
        public async Task Die()
        {
            await ReplyAsync("Going offline.");
            await Program.Instance.Client.SetStatusAsync(Discord.UserStatus.Invisible);

            Environment.Exit(0);
        }
    }
}
