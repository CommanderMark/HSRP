using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace HSRP.Commands
{
    /// <summary>
    /// Require that the command or group invoked is done by a registered player.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    class RequireRegistrationAttribute : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (Player.Registered(context.User.Id))
            {
                return PreconditionResult.FromSuccess();
            }

            await DiscordToolbox.DMUser(context.User, "You must be registered before that command can be accessed.");
            return PreconditionResult.FromError(context.User + "isn't registered.");
        }
    }
}