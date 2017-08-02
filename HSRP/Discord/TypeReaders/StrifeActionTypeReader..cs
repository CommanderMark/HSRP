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
            Type type = typeof(StrifeAction);
            foreach (FieldInfo fld in type.GetFields())
            {
                if (fld.Name.StartsWith(input, StringComparison.OrdinalIgnoreCase))
                {
                    return await Task.FromResult(TypeReaderResult.FromSuccess(fld));
                }
            }
            await context.Channel.SendMessageAsync("No such ability found.");
            return TypeReaderResult.FromError(CommandError.ParseFailed, "No such ability found.");
        }
    }
}