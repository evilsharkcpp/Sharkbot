using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace SharkBot.Commands
{
    public sealed class BaseCommands : ModuleBase<SocketCommandContext>
    {
       
        [Command("Info")]
        public async Task GetInfoAsync()
        {
            if (Context.User != Context.Guild.Owner) return;
            await Context.User.SendMessageAsync("Commands:\n" +
                "1) <Prefix>SetPrefix - Set new prefix\n" +
                "2) <Prefix>SetStdRole - Set standart role for new users\n" +
                "3) <Prefix>SetMutedRole - Set Muted role for mute users"
                 );
        }
        [Command("AddRole")]
        public async Task AddRoleAsync(string RoleName)
        {
            await Context.Guild.CreateRoleAsync(RoleName,new GuildPermissions(
                sendMessages:false),new Color(127,127,127),false,null);
        }
        [Command("getmyid")]
        public async Task GetMyIdAsync()
        {
            var user = Context.User;
            await ReplyAsync($"<@!{user.Id}>, ваш ID: `{user.Id}`");
        }
        [Command("mat")]
        public async Task MatAsync()
        {
            await ReplyAsync($"`Suka blyat`");
        }
        [Command("help")]
        public async Task HelpAsync()
        {
            await ReplyAsync(null,false,Templates.TemplateMessage($"Available commands:\n" +
                $"1)!play\n" +
                $"2)!join\n" +
                $"3)!leave\n" +
                $"4)!pause\n" +
                $"5)!stop\n" +
                $"6)!next\n" +
                $"7)!resume\n"));
        }
        //[Command("find")]
        public IGuildUser FindAsync(string name)
        {
            var users = Context.Guild.Users;
            foreach (var i in users)
                if (i.Username == name)
                {
                    //await ReplyAsync(null, false, Templates.TemplateMessage($"{i.Username}:`{i.Id}`"));
                    return i;
                }
           // await ReplyAsync(null, false, Templates.TemplateMessage("User not found"));
            return null;
        }
        [Command("send_message")]
        public async Task SendUserMessage(string name, string message,int count = 1)
        {
            var user = FindAsync(name);
            //Context.Guild.VoiceChannels
            await Context.Channel.DeleteMessageAsync(Context.Message.Id);
            for (int i = 0; i < count; i++)
                await user.SendMessageAsync(null,false,Templates.TemplateMessage($"Сообщение от: {Context.User.Username}: {message}"));
            await ReplyAsync(null, false, Templates.TemplateMessage($"Сообщение отправлено."));
        }
    }

}