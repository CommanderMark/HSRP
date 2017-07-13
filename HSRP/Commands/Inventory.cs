using Discord.Commands;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using System.Collections.Generic;
using System;

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
            if (item.CaseInsensitiveContains("equipped"))
            {
                await ReplyAsync("Nice try. :/");
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

        [Command("remove"), Priority(1), RequireGM]
        public async Task Remove(Player plyr, int index)
        {
            Item item = plyr.Inventory.ElementAtOrDefault(index);
            if (item == null)
            {
                await ReplyAsync("Invalid item index.");
                return;
            }
            plyr.Inventory.Remove(item);

            plyr.Save();
            string log = Syntax.ToCodeLine(item.name) + " was removed from the inventory of " + Syntax.ToCodeLine(plyr.Name) + ".";
            await ReplyAsync(log);
            await Program.Instance.LogChannel(Context, log);
        }

        [Command("remove"), Priority(0), RequireGM]
        public async Task Remove(Player plyr, string name)
        {
            Item item = plyr.Inventory.FirstOrDefault(x => x.name.StartsWith(name, StringComparison.OrdinalIgnoreCase));
            if (item == null)
            {
                await ReplyAsync("Invalid item name.");
                return;
            }
            plyr.Inventory.Remove(item);

            plyr.Save();
            string log = Syntax.ToCodeLine(item.name) + " was removed from the inventory of " + Syntax.ToCodeLine(plyr.Name) + ".";
            await ReplyAsync(log);
            await Program.Instance.LogChannel(Context, log);
        }

        [Command("equip"), Priority(1), RequireGM]
        public async Task Equip(Player plyr, int index)
        {
            Item prevItem = plyr.Inventory.ElementAtOrDefault(index);
            if (prevItem == null)
            {
                await ReplyAsync("Invalid item index.");
                return;
            }

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

        [Command("equip"), Priority(0), RequireGM]
        public async Task Equip(Player plyr, string name)
        {
            Item prevItem = plyr.Inventory.FirstOrDefault(x => x.name.StartsWith(name, StringComparison.OrdinalIgnoreCase));
            if (prevItem == null)
            {
                await ReplyAsync("Invalid item name.");
                return;
            }

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

        [Command("unequip"), Priority(1), RequireGM]
        public async Task UnEquip(Player plyr, int index)
        {
            Item prevItem = plyr.Inventory.ElementAtOrDefault(index);
            if (prevItem == null)
            {
                await ReplyAsync("Invalid item index.");
                return;
            }

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

        [Command("unequip"), Priority(0), RequireGM]
        public async Task UnEquip(Player plyr, string name)
        {
            Item prevItem = plyr.Inventory.FirstOrDefault(x => x.name.StartsWith(name, StringComparison.OrdinalIgnoreCase));
            if (prevItem == null)
            {
                await ReplyAsync("Invalid item name.");
                return;
            }

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
