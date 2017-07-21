using Discord.Commands;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using System.Collections.Generic;
using System;

namespace HSRP.Commands
{
    [Group("inventory"), Alias("inv")]
    public class InventoryCommands : JModuleBase
    {
        [RequireRegistration]
        [Command]
        public async Task Inv() => await Inv(new Player(Context.User));

        [RequireGM]
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

        [RequireGM]
        [Command("add")]
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
            i.quantity = 1;
            plyr.Inventory.AddLast(i);

            plyr.Save();
            string log = Syntax.ToCodeLine(item) + " was added to the inventory of " + Syntax.ToCodeLine(plyr.Name) + ".";
            await ReplyAsync(log);
        }

        [RequireGM]
        [Command("remove"), Priority(1)]
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
        }

        [RequireGM]
        [Command("remove"), Priority(0)]
        public async Task Remove(Player plyr, [Remainder] string name)
        {
            Item item = plyr.Inventory.FirstOrDefault(x => x.name.StartsWith(name, StringComparison.OrdinalIgnoreCase));
            if (item == null)
            {
                await ReplyAsync("Invalid item name.");
                return;
            }
            
            int index = plyr.Inventory.IndexOf(item);
            await Remove(plyr, index);
        }

        [RequireGM]
        [Command("equip"), Priority(1)]
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
        }

        [RequireGM]
        [Command("equip"), Priority(0)]
        public async Task Equip(Player plyr, [Remainder] string name)
        {
            Item prevItem = plyr.Inventory.FirstOrDefault(x => x.name.StartsWith(name, StringComparison.OrdinalIgnoreCase));
            if (prevItem == null)
            {
                await ReplyAsync("Invalid item name.");
                return;
            }

            int index = plyr.Inventory.IndexOf(prevItem);
            await Equip(plyr, index);
        }

        [RequireGM]
        [Command("unequip"), Priority(1)]
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
        }

        [RequireGM]
        [Command("unequip"), Priority(0)]
        public async Task UnEquip(Player plyr, [Remainder] string name)
        {
            Item prevItem = plyr.Inventory.FirstOrDefault(x => x.name.StartsWith(name, StringComparison.OrdinalIgnoreCase));
            if (prevItem == null)
            {
                await ReplyAsync("Invalid item name.");
                return;
            }

            int index = plyr.Inventory.IndexOf(prevItem);
            await UnEquip(plyr, index);
        }

        [RequireGM]
        [Group("quantity"), Alias("amount")]
        public class QuantityCommands : JModuleBase
        {
            [Command("add"), Priority(1)]
            public async Task QuantityAdd(Player plyr, int index, int amount)
            {
                Item prevItem = plyr.Inventory.ElementAtOrDefault(index);
                if (prevItem == null)
                {
                    await ReplyAsync("Invalid item index.");
                    return;
                }

                if (amount <= 0)
                {
                    await ReplyAsync("Invalid amount.");
                    return;
                }

                Item newItem = prevItem;
                newItem.quantity += (uint)amount;
                plyr.Inventory.Find(prevItem).Value = newItem;

                plyr.Save();
                string log = $"{amount} {Syntax.ToCodeLine(newItem.name)} added to the inventory of {Syntax.ToCodeLine(plyr.Name)}.";
                await ReplyAsync(log);
            }

            [Command("add"), Priority(0)]
            public async Task QuantityAdd(Player plyr, string name, int amount)
            {
                Item prevItem = plyr.Inventory.FirstOrDefault(x => x.name.StartsWith(name, StringComparison.OrdinalIgnoreCase));
                if (prevItem == null)
                {
                    await ReplyAsync("Invalid item name.");
                    return;
                }

                int index = plyr.Inventory.IndexOf(prevItem);
                await QuantityAdd(plyr, index, amount);
            }

            [Command("remove"), Priority(1)]
            public async Task QuantityRemove(Player plyr, int index, int amount)
            {
                Item prevItem = plyr.Inventory.ElementAtOrDefault(index);
                if (prevItem == null)
                {
                    await ReplyAsync("Invalid item index.");
                    return;
                }

                if (amount <= 0 || amount > prevItem.quantity)
                {
                    await ReplyAsync("Invalid amount.");
                    return;
                }

                Item newItem = prevItem;
                newItem.quantity -= (uint)amount;
                plyr.Inventory.Find(prevItem).Value = newItem;

                plyr.Save();
                string log = $"{amount} {Syntax.ToCodeLine(newItem.name)} removed from the inventory of {Syntax.ToCodeLine(plyr.Name)}.";
                await ReplyAsync(log);
            }

            [Command("remove"), Priority(0)]
            public async Task QuantityRemove(Player plyr, string name, int amount)
            {
                Item prevItem = plyr.Inventory.FirstOrDefault(x => x.name.StartsWith(name, StringComparison.OrdinalIgnoreCase));
                if (prevItem == null)
                {
                    await ReplyAsync("Invalid item name.");
                    return;
                }

                int index = plyr.Inventory.IndexOf(prevItem);
                await QuantityRemove(plyr, index, amount);
            }
        }
    }
}
