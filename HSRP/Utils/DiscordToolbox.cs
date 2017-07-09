using System;
using System.Threading.Tasks;
using System.Linq;
using Discord;

namespace HSRP
{
    public static class DiscordToolbox
    {
        public static async Task SetRole(IGuildUser user, string roleStr, bool remove = false)
        {
            foreach (IRole role in user.Guild.Roles)
            {
                if (role.Name.Equals(roleStr, StringComparison.OrdinalIgnoreCase))
                {
                    if (!remove)
                    {
                        await user.AddRoleAsync(role);
                    }
                    else
                    {
                        if (user.RoleIds.Contains(role.Id))
                        {
                            await user.RemoveRoleAsync(role);
                        }
                        else
                        {
                            Console.WriteLine("[ROLE] " + user.Username + " did not posess the role " + role.Name + " when attempting to remove it!");
                        }
                    }
                }
            }
        }

        public static async Task SetRole(IGuildUser user, ulong roleID, bool remove = false)
        {
            foreach (IRole role in user.Guild.Roles)
            {
                if (role.Id == roleID)
                {
                    if (!remove)
                    {
                        await user.AddRoleAsync(role);
                    }
                    else
                    {
                        if (user.RoleIds.Contains(role.Id))
                        {
                            await user.RemoveRoleAsync(role);
                        }
                        else
                        {
                            Console.WriteLine("[ROLE] " + user.Username + " did not posess the role " + role.Name + " when attempting to remove it!");
                        }
                    }
                }
            }
        }

        public static async Task DMUser(IUser user, string message = "", Embed embed = null)
        {
            await (await user.GetOrCreateDMChannelAsync()).SendMessageAsync(message, embed: embed);
        }

        // Extension method for getting hex from Discord.Color.
        public static string GetHexValue(this Color clr)
        {
            return clr.R.ToString("X2") + clr.G.ToString("X2") + clr.B.ToString("X2");
        }
    }
}
