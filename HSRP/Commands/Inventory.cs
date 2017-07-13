using Discord.Commands;
using System.Linq;
using System.Threading.Tasks;

namespace HSRP.Commands
{
    [Group("inventory"), Alias("inv")]
    public class InventoryCommands : ModuleBase
    {
        [Command, RequireRegistration]
        public async Task Inv() => await Inv(new Player(Context.User));

        [Command]
        public async Task Inv(Player plyr)
        {
            string output = "Inventory for " + plyr.Name + ":\n";
            if (plyr.Inventory.Any())
            {
                output += Syntax.ToCodeBlock(plyr.DisplayInventory());
            }
            else
            {
                output += Syntax.ToCodeBlock("There is nothing in this inventory.");
            }

            await ReplyAsync(output);
        }

        [Command("add"), RequireGM]
        public async Task Add(Player plyr, string item)
        {
            Item i = new Item();
            i.name = item;
            plyr.Inventory.AddLast(i);

            plyr.Save();
            string log = item.FirstCharUpper() + "was added to the inventory of " + plyr.Name + ".";
            await ReplyAsync(log);
            await Program.Instance.LogChannel(Context, log);
        }
    }
}
