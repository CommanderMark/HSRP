using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using System.Collections.Generic;
using System;

namespace HSRP.Commands
{
    public class JModuleBase : ModuleBase
    {
        public async Task ReplyStrifeAsync(string msg)
        {
            ITextChannel chnl = await Program.Instance.RpGuild.GetTextChannelAsync(Constants.STRIFE_CHANNEL);
            await chnl.SendMessageAsync(msg);
        }

        /// <summary>
        /// Sends a message to the channel where the command was executed. If the message exceeds Discord's character limit then it segments the message across multiple.
        /// </summary>
        public async Task ReplyStrifeSegmentAsync(string msg)
        {
            if (msg.Length > Constants.DiscordCharLimit)
            {
                for (int i = 0; i < msg.Length; i += Constants.DiscordCharLimit)
                {
                    string segment = msg.Substring(i, Math.Min(i + Constants.DiscordCharLimit, msg.Length - i));
                    await ReplyStrifeAsync(segment);
                }
            }
            else
            {
                await ReplyStrifeAsync(msg);
            }
        }
    }
}