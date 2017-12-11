using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace HSRP.Commands
{
    class StrifeTypeReader : TypeReader
    {
        public override async Task<TypeReaderResult> Read(ICommandContext context, string input, IServiceProvider services)
        {
            if (int.TryParse(input, out int id))
            {
                Strife strf = new Strife(id.ToString());
                if (!strf.Errored)
                {
                    return await Task.FromResult(TypeReaderResult.FromSuccess(strf));
                }
            }
            
            return TypeReaderResult.FromError(CommandError.ParseFailed, "No such strife was found.");
        }
    }
}