using System.Threading.Tasks;
using Discord.Commands;

namespace HSRP.Commands
{
    // Stuff for modifying pre-existing player data.
    [Group("edit")]
    public class EditCommands : JModuleBase
    {
        [RequireRegistration]
        [Command("name"), Priority(0)]
        public async Task Name([Remainder] string name) => await Name(new Player(Context.User), name);

        [RequireGM]
        [Command("name"), Priority(1)]
        public async Task Name(Player plyr, [Remainder] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                await ReplyAsync("Invalid name.");
                return;
            }

            string prev = plyr.Name;
            plyr.Name = name;
            plyr.Save();

            await ReplyAsync(Syntax.ToCodeLine(prev) + " was renamed to " + Syntax.ToCodeLine(plyr.Name) + ".");
        }

        [RequireRegistration]
        [Command("specibus"), Priority(0)]
        public async Task Specibus([Remainder] string name) => await Specibus(new Player(Context.User), name);

        [RequireGM]
        [Command("specibus"), Priority(1)]
        public async Task Specibus(Player plyr, [Remainder] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                await ReplyAsync("Invalid specibus.");
                return;
            }

            string prev = plyr.Specibus;
            plyr.Specibus = name;
            plyr.Save();

            await ReplyAsync($"{Syntax.ToCodeLine(plyr.Name)} changed their specibus. ({Syntax.ToCodeLine(prev)} -> {Syntax.ToCodeLine(plyr.Specibus)})");
        }

        [RequireGM(Group = "selection"), RequireRegistration(Group = "selection")]
        [Command("pineapple"), Alias("pineappleonpizza")]
        public async Task Pineapple([Remainder] string any)
        {
            await ReplyAsync("You cannot change what is true.");
        }
    }
}