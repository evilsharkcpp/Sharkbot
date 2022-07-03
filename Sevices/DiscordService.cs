using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using SharkBot.Data;
using SharkBot.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Victoria;
using static SharkBot.Templates;
namespace SharkBot.Sevices
{
    class DiscordService
    {
        private DiscordSocketClient client;
        private CommandService commands;
        private IServiceProvider services;
        private LavaNode _instanceOfLavaNode;
        private MusicService _musicService;
        private ConfigService _configService;
        private ServerService _serverService;
        void InitEvents()
        {
            client.Log += Log;
            client.MessageReceived += HandleCommandAsync;
            client.Ready += OnReadyAsync;
            client.SetGameAsync(BotSetup.Config.GameStatus);
            client.UserJoined += UserJoinedAsync;
            client.JoinedGuild += JoinedGuildAsync;
            //client.GuildMembersDownloaded += GuildMembersDownloaded;
            client.GuildAvailable += GuildAvailable;
            var newTask = new Task(async () => await CheckTime());
            newTask.Start();
        }

        private Task GuildAvailable(SocketGuild arg)
        {
            if (!_configService.guildSetups.ContainsKey(arg.Id))
            {
                _configService.guildSetups.Add(arg.Id, new GuildSetup($"{arg.Id}.json"));
            }
            return Task.CompletedTask;
        }

        private Task JoinedGuildAsync(SocketGuild arg)
        {
            if (!_configService.guildSetups.ContainsKey(arg.Id))
            {
                _configService.guildSetups.Add(arg.Id, new GuildSetup($"{arg.Id}.json"));
            }
            return Task.CompletedTask;
        }

        public async Task RunBotAsync()
        {

            var cfg = new DiscordSocketConfig()
            {
                LogLevel = LogSeverity.Debug,
                AlwaysDownloadUsers = true,
            };
            cfg.GatewayIntents |= GatewayIntents.GuildMembers;
            client = new DiscordSocketClient(cfg);
            commands = new CommandService(new CommandServiceConfig()
            {
                LogLevel = LogSeverity.Debug,
                CaseSensitiveCommands = true,
                DefaultRunMode = RunMode.Async,
                IgnoreExtraArgs = true,
            });

            _instanceOfLavaNode = new LavaNode(client, new LavaConfig()
            {
                LogSeverity = LogSeverity.Debug
            });
            _musicService = new MusicService(_instanceOfLavaNode);
            _musicService.InitEvents();
            _configService = new ConfigService();
            _serverService = new ServerService(_configService);
            services = new ServiceCollection()
                .AddSingleton(client)
                .AddSingleton(commands)
                .AddSingleton(_instanceOfLavaNode)
                .AddSingleton(_musicService)
                .AddSingleton(_serverService)
                .BuildServiceProvider();
            InitEvents();
            await commands.AddModulesAsync(Assembly.GetEntryAssembly(), services);
            await client.LoginAsync(TokenType.Bot, BotSetup.Config.Token);

            await client.StartAsync();
            Thread thread = new Thread(Exit);
            thread.Start();

        }
        //Some dep. functions
        public async Task CheckTime()
        {
            
            var guildSetups = _configService.guildSetups;
            foreach (var item in guildSetups)
            {
                foreach (var i in item.Value.users)
                {
                    if (DateTime.Now >= i.TimeEnded)
                    {
                        var guild = client.GetGuild(item.Key);
                        var user = guild.GetUser(i.Id);

                        if (user != null && guild != null)
                        {
                            if (guild.CurrentUser.Hierarchy > user.Hierarchy)
                            await user.RemoveRoleAsync(item.Value.Config.MutedRoleId);
                            i.TimeEnded = DateTime.MaxValue;
                        }
                    }
                }
            }
            await Task.Delay(60 * 1000);
            await CheckTime();
        }
        public void Exit()
        {
            while (Console.ReadLine().ToLower() != "exit") ;
            _instanceOfLavaNode.DisconnectAsync();
            client.StopAsync();
        }
        [RequireBotPermission(GuildPermission.KickMembers)]
        public async Task Filter(SocketUserMessage msg, ulong guildId)
        {
            var message = msg.Content;
            var guild = client.GetGuild(guildId);
            foreach (var item in _configService.guildSetups[guildId].Config.WrongWords)
            {
                if (message.ToLower().Contains(item.ToLower()))
                {

                    foreach (var i in _configService.guildSetups[guildId].users)
                    {
                        if (i.Id == msg.Author.Id)
                        {
                            i.ViolationCount++;
                            var user = guild.GetUser(msg.Author.Id);
                            if (user == null)
                            {
                                await guild.DefaultChannel.SendMessageAsync("User not found");
                                return;
                            }
                            if (i.ViolationCount > 4)
                            {
                                
                                if (!guild.CurrentUser.GuildPermissions.BanMembers)
                                {
                                    await guild.DefaultChannel.SendMessageAsync($"I can't ban User({user.Username}), no have permission");
                                    return;
                                }
                                if (guild.CurrentUser.Hierarchy < user.Hierarchy)
                                {
                                    await guild.DefaultChannel.SendMessageAsync($"My hierarchy lower that user");
                                    return;
                                }
                                await user.BanAsync(Convert.ToInt32(i.ViolationCount));
                                await guild.DefaultChannel.SendMessageAsync($"{msg.Author.Username} no swear, I ban you now");
                            }
                            if (i.ViolationCount > 3)
                            {
                                if (!guild.CurrentUser.GuildPermissions.KickMembers)
                                {
                                    await guild.DefaultChannel.SendMessageAsync($"I can't kick User({user.Username}), no have permission");
                                    return;
                                }
                                await user.KickAsync("No swear!!!!");
                            }
                            if (i.ViolationCount > 0)
                            { 
                                i.TimeEnded = DateTime.Now.AddMinutes(i.ViolationCount * 30);
                                if (guild.GetRole(_configService.guildSetups[guildId].Config.MutedRoleId) == null)
                                {
                                    await guild.DefaultChannel.SendMessageAsync("Muted role don't exist or found");
                                    return;
                                }
                                if (guild.CurrentUser.Hierarchy > user.Hierarchy && guild.GetRole(_configService.guildSetups[guildId].Config.MutedRoleId).Position < guild.CurrentUser.Hierarchy)
                                {
                                    await guild.GetUser(i.Id).AddRoleAsync(guild.GetRole(_configService.guildSetups[guildId].Config.MutedRoleId));
                                    await guild.DefaultChannel.SendMessageAsync(embed: GetUserProfile(guild.GetUser(i.Id), i.TimeEnded));
                                }
                                else
                                    await guild.DefaultChannel.SendMessageAsync($"My role hierarchy lower that Muted role or User({user.Username}) Hierarchy higher that me");
                            }

                        }
                    }
                    _configService.guildSetups[guildId].users.Add(new UserInfo(msg.Author.Id));
                }
            }

        }

        //Events in guild
        private async Task UserJoinedAsync(SocketGuildUser arg)
        {
            await Log(new LogMessage(LogSeverity.Info, arg.Username + arg.Discriminator, "Joined in server"));
            var user = arg as IGuildUser;
            await arg.Guild.DefaultChannel.SendMessageAsync($"{arg.Username + arg.Discriminator} Joined in server");
            //await arg.AddRoleAsync(_configService.guildSetups[arg.Guild.Id].Config.stdRole);
            await user.AddRoleAsync(_configService.guildSetups[arg.Guild.Id].Config.StdRole);
            //await arg.AddRoleAsync(arg.Guild.GetRole(_configService.guildSetups[arg.Guild.Id].Config.stdRole));
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            await Log(new LogMessage(LogSeverity.Info, arg.Author.Username + arg.Author.Discriminator, arg.Content));
            if (!(arg is SocketUserMessage message)) return;
            var context = new SocketCommandContext(client, message);
            if (message.Author.IsBot) return;
            var Prefix = "";
            if (_configService.guildSetups.ContainsKey(context.Guild.Id))
                Prefix = _configService.guildSetups[context.Guild.Id].Config.Prefix;
            else
            {
                _configService.guildSetups.Add(context.Guild.Id, new GuildSetup($"{context.Guild.Id}.json"));
                Prefix = BotSetup.Config.Prefix;
            }
            int argPos = 0;
            await Filter(message, context.Guild.Id);
            if (message.HasStringPrefix(Prefix, ref argPos))
            {
                var result = await commands.ExecuteAsync(context, argPos, services);
                if (!result.IsSuccess)
                    Console.WriteLine(result.ErrorReason);
                if (result.Error.Equals(CommandError.UnmetPrecondition))
                    await message.Channel.SendMessageAsync(result.ErrorReason);
            }
        }
        private async Task OnReadyAsync()
        {
            if (!_instanceOfLavaNode.IsConnected)
            {
                await _instanceOfLavaNode.ConnectAsync();
            }
        }
        public static Task Log(LogMessage message)
        {
            switch (message.Severity)
            {
                case LogSeverity.Critical:
                case LogSeverity.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogSeverity.Info:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case LogSeverity.Verbose:
                case LogSeverity.Debug:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    break;
            }
            Console.WriteLine($"{DateTime.Now,-19} [{message.Severity}] {message.Source}: {message.Message} {message.Exception}");
            Console.ResetColor();
            return Task.CompletedTask;
        }
    }
}
