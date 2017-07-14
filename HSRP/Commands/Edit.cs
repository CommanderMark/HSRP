using System.Threading.Tasks;
using Discord.Commands;

namespace HSRP.Commands
{
    // Stuff for modifying pre-existing player data.
    [Group("edit"), RequireRegistration]
    public class EditCommands : ModuleBase
    {
        [Command("name"), RequireRegistration, Priority(1)]
        public async Task Name([Remainder] string name) => await Name(new Player(Context.User), name);

        [Command("name"), RequireGM]
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

        [Command("specibus"), RequireRegistration, Priority(1)]
        public async Task Specibus([Remainder] string name) => await Specibus(new Player(Context.User), name);

        [Command("specibus"), RequireGM]
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

        [Command("lusus"), RequireRegistration, Priority(1)]
        public async Task Lusus([Remainder] string desc) => await Lusus(new Player(Context.User), desc);

        [Command("lusus"), RequireGM]
        public async Task Lusus(Player plyr, [Remainder] string desc)
        {
            if (string.IsNullOrWhiteSpace(desc))
            {
                await ReplyAsync("Invalid description.");
                return;
            }

            plyr.LususDescription = desc;
            plyr.Save();

            await ReplyAsync(Syntax.ToCodeLine(plyr.Name) + " lusus description to updated.");
        }

        [Command("pineapple"), Alias("pineappleonpizza"), RequireGM(Group = "selection"), RequireRegistration(Group = "selection")]
        public async Task Pineapple([Remainder] string any)
        {
            await ReplyAsync("You cannot change what is true.");
        }
    }
}