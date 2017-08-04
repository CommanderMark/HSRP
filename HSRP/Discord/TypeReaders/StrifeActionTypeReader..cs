using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace HSRP.Commands
{
    class StrifeActionTypeReader : TypeReader
    {
        public override async Task<TypeReaderResult> Read(ICommandContext context, string input, IServiceProvider services)
        {
            var fields = Enum.GetValues(typeof(StrifeAction)).Cast<StrifeAction>();
            foreach (StrifeAction sa in fields)
            {
                if (sa.ToString().StartsWith(input, StringComparison.OrdinalIgnoreCase))
                {
                    return await Task.FromResult(TypeReaderResult.FromSuccess(sa));
                }
            }
            await context.Channel.SendMessageAsync("No such command found.");
            return TypeReaderResult.FromError(CommandError.ParseFailed, "No such command found.");
        }
    }
}