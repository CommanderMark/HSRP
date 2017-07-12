using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace HSRP.Commands
{
    class AbilityTypeReader : TypeReader
    {
        public override async Task<TypeReaderResult> Read(ICommandContext context, string input, IServiceProvider services)
        {
            Type type = typeof(AbilitySet);
            foreach (PropertyInfo prop in type.GetProperties())
            {
                if (input.StartsWith(prop.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return await Task.FromResult(TypeReaderResult.FromSuccess(prop));
                }
            }
            await context.Channel.SendMessageAsync("No such ability found.");
            return TypeReaderResult.FromError(CommandError.ParseFailed, "No such ability found.");
        }
    }
}