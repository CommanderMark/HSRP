using System;
using System.Threading.Tasks;
using System.Linq;
using Discord;
using Discord.Commands;
using System.IO;

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

        // Extension methods.
        public static string GetHexValue(this Color clr)
        {
            return clr.R.ToString("X2") + clr.G.ToString("X2") + clr.B.ToString("X2");
        }

        public static Player GetPlayerEntity(this ICommandContext context)
        {
            Player plyr = new Player(context.User);
            return plyr.Errored ? null : plyr;
        }

        public static Strife GetStrife(this ICommandContext context)
        {
            Player plyr = context.GetPlayerEntity();
            if (plyr == null) { return null; }

            foreach (string dir in Directory.GetFiles(Dirs.Strifes))
            {
                if (dir.Contains(plyr.StrifeID.ToString()))
                {
                    return new Strife(dir);
                }
            }

            return null;
        }
    }

    // Syntax stuff for Discord.
    public static class Syntax
    {
        public static string ToItalics(string text) => "_" + text + "_";
        public static string ToBold(string text) => "**" + text + "**";
        public static string ToBoldItalics(string text) => ToBold(ToItalics(text));
        public static string ToStrikethrough(string text) => "~~" + text + "~~";

        public static string ToCodeLine(string text) => "`" + text + "`";
        public static string ToCodeBlock(string text) => "```" + text + "```";
    }
}
