using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace HSRP.Commands
{
    class PlayerTypeReader : TypeReader
    {
        public override async Task<TypeReaderResult> Read(ICommandContext context, string input, IServiceProvider services)
        {
            (bool, Player) tuple = await Player.TryParse(input);
            if (tuple.Item1)
            {
                return await Task.FromResult(TypeReaderResult.FromSuccess(tuple.Item2));
            }

            return TypeReaderResult.FromError(CommandError.ParseFailed, "No such player was found.");
        }
    }
}