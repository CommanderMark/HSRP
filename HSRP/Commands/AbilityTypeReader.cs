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
            Type type = typeof(BaseAbility);
            foreach (FieldInfo field in type.GetFields())
            {
                if (input.StartsWith(field.Name) && Enum.TryParse(field.Name, out BaseAbility result))
                {
                    return await Task.FromResult(TypeReaderResult.FromSuccess(result));
                }
            }
            await context.Channel.SendMessageAsync("No such ability found.");
            return TypeReaderResult.FromError(CommandError.ParseFailed, "No such ability found.");
        }
    }
}