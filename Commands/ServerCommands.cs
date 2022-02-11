using Discord;
using Discord.Commands;
using SharkBot.Sevices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharkBot.Commands
{
    public class ServerCommands:ModuleBase<SocketCommandContext>
    {
        readonly ServerService serverService;
        [Command("Profile")]
        public async Task GetProfileAsync()
        {
            await serverService.GetProfile(Context.Guild, Context.User.Id, Context.Channel as ITextChannel);
        }
        public ServerCommands(ServerService service) => serverService = service;
        [Command("AddBadWord")]
        public async Task AddBadWord(string badWord)
        {
            serverService.AddBadWord(Context.Guild.Id, badWord);
            await ReplyAsync("Word added in list of forbidden words");
        }
        [Command("SetPrefix")]
        public async Task SetPrefixAsync(string prefix)
        {
            try
            {
                serverService.SetPrefixAsync(Context.Guild.Id, prefix);
                await ReplyAsync("`Prefix changed`");
            }
            catch (Exception ex)
            {
                await ReplyAsync("`Oops!Something went wrong`");
                await DiscordService.Log(new Discord.LogMessage(Discord.LogSeverity.Error, "Bot", ex.Message));
            }
        }
        [Command("SetStdRole")]
        public async Task SetStdRoleAsync(string roleName)
        {
            var Roles = Context.Guild.Roles;
            ulong roleId;
            foreach (var item in Roles)
            {
                if (item.Name == roleName)
                {
                    roleId = item.Id;
                    await ReplyAsync($"Role setup, role id: {roleId}");
                    serverService.SetStdRoleAsync(Context.Guild.Id, roleId);
                    return;
                }
            }
            await ReplyAsync("Role not found");
        }
        [Command("SetMutedRole")]
        public async Task SetMutedRoleAsync(string roleName)
        {
            var Roles = Context.Guild.Roles;
            foreach (var item in Roles)
                if(item.Name == "Muted" || item.Name == roleName)
                {
                    await ReplyAsync($"`Muted role set`");
                    serverService.SetMutedRoleAsync(Context.Guild.Id, item.Id);
                    return;
                }
            await ReplyAsync("`Role not found`");
        }
        [Command("get_roleId")]
        public async Task GetRoleIdAsync(string roleName)
        {
            var Roles = Context.Guild.Roles;
            ulong roleId;
            foreach (var item in Roles)
            {
                if (item.Name == roleName)
                {
                    roleId = item.Id;
                    await ReplyAsync($"role id: {roleId}");
                    return;
                }
            }
            await ReplyAsync("Role not found");
        }
        [Command("set_roleId")]
        public async Task SetRoleIdAsync(string userName, string roleName)
        {
            var Roles = Context.Guild.Roles;
            var Users = Context.Guild.Users;
            ulong roleId;
            foreach (var item in Roles)
            {
                if (item.Name == roleName)
                {
                    roleId = item.Id;
                    await ReplyAsync($"role id: {roleId}");
                    foreach(var user in Users)
                    {
                        if(user.Username == userName)
                        {
                            await user.AddRoleAsync(roleId);
                            return;
                        }
                    }
                }
            }
            await ReplyAsync("Role not found");
        }

    }
}
