using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord;

namespace HSRP.Commands
{
    /// <summary>
    /// Require that the command or group invoked is done by a user with the GM role.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    class InStrifeAttribute : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            Strife strf = context.GetStrife();
            if (strf != null)
            {
                return PreconditionResult.FromSuccess();
            }

            await DiscordToolbox.DMUser(context.User, "You are not in a strife.");
            return PreconditionResult.FromError(context.User + "isn't in a strife.");
        }
    }
}