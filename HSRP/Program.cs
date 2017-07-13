﻿using System;
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
                LogLevel = LogSeverity.Verbose
                
            });

            // Add the log handler.
            Client.Log += async (message) => await Console.Out.WriteLineAsync($"[{message.Severity}] {message.Source} -> {message.Message}");

            // Add the command service.
            commands = new CommandService(new CommandServiceConfig()
            {
                DefaultRunMode = RunMode.Sync,
                LogLevel = LogSeverity.Error
            });
            // FUCK YES
            commands.Log += async (message) => await Console.Out.WriteLineAsync($"[{message.Severity}] {message.Source} -> {message.Message}\n\n" + message.Exception);

            Registers = new Dictionary<ulong, int>();

            await Client.LoginAsync(TokenType.Bot, File.ReadAllText(Path.Combine(Dirs.Config, "token.txt")));
            await Client.StartAsync();

            // Hook the MessageReceived Event into our Command Handler.
            Client.MessageReceived += OnMessageReceive;
            Client.Connected += OnConnect;
            Client.Ready += OnReady;
            Client.JoinedGuild += OnJoinGuild;
            // Client.UserJoined += OnUserJoin;
            // Client.UserLeft += OnUserLeave;

            // Discover all of the commands in this assembly and load them.
            commands.AddTypeReader<Player>(new Commands.PlayerTypeReader());
            commands.AddTypeReader<PropertyInfo>(new Commands.AbilityTypeReader());
            await commands.AddModulesAsync(Assembly.GetEntryAssembly());

            // Block this task until the program is exited.
            await Task.Delay(-1);
        }

        public async Task OnConnect()
        {
            await Client.SetGameAsync("Beta Test");
        }

        public async Task OnReady()
        {
            try
            {
                Instance.RpGuild = Client.GetGuild(Constants.RP_GUILD) as IGuild;
                await Console.Out.WriteLineAsync("Hello!");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public async Task OnJoinGuild(SocketGuild guild)
        {
            if (guild.Id != Constants.RP_GUILD)
            {
                await guild.DefaultChannel.SendMessageAsync("This bot only works on its original server."
                    + "\nIf you see this message then either you're doing something wrong or Mark fucked up.");
                await guild.LeaveAsync();
            }
        }

        public async Task OnUserJoin(SocketGuildUser user)
        {
            SocketTextChannel chnl = user.Guild.GetTextChannel(Constants.GEN_CHANNEL);
            await chnl.SendMessageAsync(user.Username + " has joined the server.");
        }

        public async Task OnUserLeave(SocketGuildUser user)
        {
            SocketTextChannel chnl = user.Guild.GetTextChannel(Constants.GEN_CHANNEL);
            await chnl.SendMessageAsync(user.Username + " has left the server.");
        }

        public async Task OnMessageReceive(SocketMessage messageParam)
        {
            // Don't process the command if it was a System Message.
            SocketUserMessage message = messageParam as SocketUserMessage;
            if (message == null) { return; }

            // Create command context.
            CommandContext context = new CommandContext(Client, message);

            // Create a number to track where the prefix ends and the command begins.
            int argPos = 0;

            if (message.HasStringPrefix(Constants.BotPrefix, ref argPos))
            {
                IResult result = await commands.ExecuteAsync(context, argPos);

                if (!result.IsSuccess)
                {
                    Toolbox.DebugWriteLine("[WHOOPS]", result.ErrorReason);
                }
            }
        }

        public async Task LogChannel(ICommandContext context, string msg)
        {
            ITextChannel chnl = await RpGuild.GetTextChannelAsync(Constants.LOG_CHANNEL);
            string result = "[" + context.User.Username + "]\n";
            result += msg;

            await chnl.SendMessageAsync(result);
        }
    }
}