using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace HSRP.Commands
{
    class EntityTypeReader : TypeReader
    {
        public override async Task<TypeReaderResult> Read(ICommandContext context, string input, IServiceProvider services)
        {
            // Player?
            Tuple<bool, Player> tuple = await Player.TryParse(input);
            if (tuple.Item1)
            {
                return await Task.FromResult(TypeReaderResult.FromSuccess(tuple.Item2));
            }

            // NPC?
            if (NPC.TryParse(input, out NPC npc))
            {
                return await Task.FromResult(TypeReaderResult.FromSuccess(npc));
            }
            
            return TypeReaderResult.FromError(CommandError.ParseFailed, "No such entity was found.");
        }
    }
}