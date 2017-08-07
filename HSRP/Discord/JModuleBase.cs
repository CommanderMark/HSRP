using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace HSRP.Commands
{
    public class JModuleBase : ModuleBase
    {
        public async Task ReplyStrifeAsync(string msg)
        {
            ITextChannel chnl = await Program.Instance.RpGuild.GetTextChannelAsync(Constants.STRIFE_CHANNEL);
            await chnl.SendMessageAsync(msg);
        }
    }
}