using System;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using System.Collections.Generic;

namespace HSRP
{
    class Program
    {
        public static Program Instance;

        // Create a DiscordClient with WebSocket support
        public DiscordSocketClient Client;
        private CommandService commands;

        public IGuild RpGuild { get; private set; }

        /// <summary>
        /// Key value pair of users who are in the process of registering.
        /// </summary>
        public Dictionary<ulong, int> Registers { get; private set; }

        // Convert our sync-main to an async main method.
        private static void Main(string[] args)
        {
            Instance = new Program();
            Instance.Run(args).GetAwaiter().GetResult();
        }

        public async Task Run(string[] args)
        {
            // Create the client.
            Client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Info
            });

            // Add the log handler.
            Client.Log += async (message) => await Console.Out.WriteLineAsync($"[{message.Severity}] {message.Source} -> {message.Message}");

            // Add the command service.
            commands = new CommandService(new CommandServiceConfig()
            {
                DefaultRunMode = RunMode.Sync,
                LogLevel = LogSeverity.Error
            });

            // If a command throws an exception then log it in the console.
            commands.Log += async (message) => await Console.Out.WriteLineAsync($"[{message.Severity}] {message.Source} -> {message.Message}\n\n" + message.Exception);

            Registers = new Dictionary<ulong, int>();

            await Client.LoginAsync(TokenType.Bot, File.ReadAllText(Path.Combine(Dirs.Config, "token.txt")));
            await Client.StartAsync();

            // Hook the MessageReceived Event into our Command Handler.
            Client.MessageReceived += OnMessageReceive;
            Client.Connected += OnConnect;
            Client.Ready += OnReady;
            Client.JoinedGuild += OnJoinGuild;

            // Discover all of the commands in this assembly and load them.
            commands.AddTypeReader<Player>(new Commands.PlayerTypeReader());
            commands.AddTypeReader<Entity>(new Commands.EntityTypeReader());
            commands.AddTypeReader<Strife>(new Commands.StrifeTypeReader());
            commands.AddTypeReader<PropertyInfo>(new Commands.AbilityTypeReader());
            await commands.AddModulesAsync(Assembly.GetEntryAssembly());

            // Block this task until the program is exited.
            await Task.Delay(-1);
        }

        public async Task OnConnect()
        {
            await Client.SetGameAsync("RP Bot");
        }

        public async Task OnReady()
        {
            await Task.Run(() =>
            {
                try
                {
                    Instance.RpGuild = Client.GetGuild(Constants.RP_GUILD) as IGuild;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });
        }

        public async Task OnJoinGuild(SocketGuild guild)
        {
            // If it's not the RP's guild then leave the server.
            if (guild.Id != Constants.RP_GUILD)
            {
                await guild.DefaultChannel.SendMessageAsync("This bot only works on its original server."
                    + "\nIf you see this message then either you're doing something wrong or the bot owner fucked up.");
                await guild.LeaveAsync();
            }
        }

        public async Task OnMessageReceive(SocketMessage messageParam)
        {
            // Don't process the command if it was a System Message.
            SocketUserMessage message = messageParam as SocketUserMessage;
            if (message == null) { return; }

            CommandContext context = new CommandContext(Client, message);

            // Create a number to track where the prefix ends and the command begins.
            int argPos = 0;

            if (message.HasStringPrefix(Constants.BotPrefix, ref argPos))
            {
                await Console.Out.WriteLineAsync($"[{message.Author}] [{message.Channel.Name}] -> {message.Content}");
                IResult result = await commands.ExecuteAsync(context, argPos);
            }
            else if (context.IsPrivate && !message.Author.IsBot)
            {
                await Console.Out.WriteLineAsync($"[{message.Channel.Name}] -> {message.Content}");
            }
        }
    }
}