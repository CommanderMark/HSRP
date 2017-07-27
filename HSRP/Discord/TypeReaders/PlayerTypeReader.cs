using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace HSRP.Commands
{
    class PlayerTypeReader : TypeReader
    {
        public override async Task<TypeReaderResult> Read(ICommandContext context, string input, IServiceProvider services)
        {
            ulong id;

            // By Mention
            if (MentionUtils.TryParseUser(input, out id))
            {
                goto Finish;
            }

            // By Id
            else if (ulong.TryParse(input, NumberStyles.None, CultureInfo.InvariantCulture, out id))
            {
                goto Finish;
            }
            else
            {
                var guildUsers = await Program.Instance.RpGuild.GetUsersAsync();

                // By Username + Discriminator
                int index = input.LastIndexOf('#');
                if (index >= 0)
                {
                    string username = input.Substring(0, index);
                    ushort discriminator;
                    if (ushort.TryParse(input.Substring(index + 1), out discriminator))
                    {
                        IGuildUser user = guildUsers.FirstOrDefault(x => x.DiscriminatorValue == discriminator &&
                            string.Equals(username, x.Username, StringComparison.OrdinalIgnoreCase));
                        if (user != null)
                        {
                            id = user.Id;
                            goto Finish;
                        }
                    }
                }


                // By Username
                // If there's more then one user with that username respond with a warning.
                var matchedUsers = guildUsers.Where(x => x.Username.Contains(input, StringComparison.OrdinalIgnoreCase));
                if (matchedUsers.Count() == 1)
                {
                    IGuildUser user = matchedUsers.FirstOrDefault();
                    if (user != null)
                    {
                        id = user.Id;
                        goto Finish;
                    }
                }
                // Multiple people with that username exist on the server.
                else if (matchedUsers.Count() > 1)
                {
                    await context.Channel.SendMessageAsync("Multiple users with that username exist on the server. Please specify their discriminator value (Username#XXXX).");
                    string users = "Users found matching that username:";
                    foreach (IGuildUser u in matchedUsers)
                    {
                       users += "\n" + u.Username + "#" + u.Discriminator;
                    }
                    await context.Channel.SendMessageAsync(users);

                    return TypeReaderResult.FromError(CommandError.MultipleMatches, "Multiple users found matching the parameters.");
                }

                // By Nickname
                matchedUsers = guildUsers.Where(x => !string.IsNullOrWhiteSpace(x.Nickname) && x.Nickname.Contains(input, StringComparison.OrdinalIgnoreCase));
                if (matchedUsers.Count() == 1)
                {
                    IGuildUser user = matchedUsers.FirstOrDefault();
                    if (user != null)
                    {
                        id = user.Id;
                        goto Finish;
                    }
                }
                // Multiple people with that username exist on the server.
                else if (matchedUsers.Count() > 1)
                {
                    await context.Channel.SendMessageAsync("Multiple users with that username exist on the server. Please specify their discriminator value (Username#XXXX).");
                    string users = "Users found matching that nickname:";
                    foreach (IGuildUser u in matchedUsers)
                    {
                       users += "\n" + u.Username + "#" + u.Discriminator;
                    }
                    await context.Channel.SendMessageAsync(users);

                    return TypeReaderResult.FromError(CommandError.MultipleMatches, "Multiple users found matching the parameters.");
                }
            }
            
            return TypeReaderResult.FromError(CommandError.ParseFailed, "No such player was found.");

        Finish:
            {
                if (id != 0 && Player.Registered(id))
                {
                    Player spe = new Player(id);
                    if (!spe.Errored)
                    {
                        return await Task.FromResult(TypeReaderResult.FromSuccess(spe));
                    }
                }
                
                return TypeReaderResult.FromError(CommandError.ParseFailed, "No such player was found.");
            }
        }
    }
}