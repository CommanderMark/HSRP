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
    class RequireGMAttribute : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            IGuildUser user = await Program.Instance.RPGuild.GetUserAsync(context.User.Id);
            foreach (ulong role in user.RoleIds)
            {
                if (role == Constants.GM_ROLE)
                {
                    return PreconditionResult.FromSuccess();
                }
            }

            return PreconditionResult.FromError(context.User + "isn't registered.");
        }
    }
}