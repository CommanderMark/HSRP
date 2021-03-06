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
            return await Task.Run<PreconditionResult>(() =>
            {
                Strife strf = context.GetStrife();
                if (strf != null && strf.Active)
                {
                    return PreconditionResult.FromSuccess();
                }
                
                return PreconditionResult.FromError(context.User + "isn't in a strife.");
            });
        }
    }
}