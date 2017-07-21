using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord;

namespace HSRP.Commands
{
    /// <summary>
    /// Require that the command or group invoked is done by Jorge.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    class RequireJorgeAttribute : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (context.User.Id == Constants.OWNER)
            {
                return PreconditionResult.FromSuccess();
            }

            await DiscordToolbox.DMUser(context.User, "You must be registered before that command can be accessed.");
            return PreconditionResult.FromError(context.User + "isn't registered.");
        }
    }
}