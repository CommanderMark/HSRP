using Discord.Commands;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using System.Collections.Generic;

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
        public async Task Add(Player plyr, [Remainder] string item)
        {
            if (string.IsNullOrWhiteSpace(item))
            {
                await ReplyAsync("Invalid item name.");
                return;
            }

            Item i = new Item();
            i.name = item;
            plyr.Inventory.AddLast(i);

            plyr.Save();
            string log = Syntax.ToCodeLine(item) + " was added to the inventory of " + Syntax.ToCodeLine(plyr.Name) + ".";
            await ReplyAsync(log);
            await Program.Instance.LogChannel(Context, log);
        }

        [Command("remove"), RequireGM]
        public async Task Remove(Player plyr, int index)
        {
            Item item = plyr.Inventory.ElementAt(index);
            plyr.Inventory.Remove(item);

            plyr.Save();
            string log = Syntax.ToCodeLine(item.name) + " was removed from the inventory of " + Syntax.ToCodeLine(plyr.Name) + ".";
            await ReplyAsync(log);
            await Program.Instance.LogChannel(Context, log);
        }

        [Command("equip"), RequireGM]
        public async Task Equip(Player plyr, int index)
        {
            Item prevItem = plyr.Inventory.ElementAt(index);

            if (prevItem.equipped)
            {
                await ReplyAsync("This item is already equipped.");
                return;
            }

            Item newItem = prevItem;
            newItem.equipped = true;
            plyr.Inventory.Find(prevItem).Value = newItem;

            plyr.Save();
            string log = Syntax.ToCodeLine(newItem.name) + " was equipped to " + Syntax.ToCodeLine(plyr.Name) + ".";
            await ReplyAsync(log);
            await Program.Instance.LogChannel(Context, log);
        }

        [Command("unequip"), RequireGM]
        public async Task UnEquip(Player plyr, int index)
        {
            Item prevItem = plyr.Inventory.ElementAt(index);

            if (!prevItem.equipped)
            {
                await ReplyAsync("This item is not equipped.");
                return;
            }

            Item newItem = prevItem;
            newItem.equipped = false;
            plyr.Inventory.Find(prevItem).Value = newItem;

            plyr.Save();
            string log = Syntax.ToCodeLine(newItem.name) + " was un-equipped from " + Syntax.ToCodeLine(plyr.Name) + ".";
            await ReplyAsync(log);
            await Program.Instance.LogChannel(Context, log);
        }
    }
}
