using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;
using Victoria;
using SharkBot.Services;
using static SharkBot.Templates;
namespace SharkBot.Commands
{
    public sealed class MusicModule : ModuleBase<SocketCommandContext>
    {
        private readonly LavaNode _lavaNode;
        private readonly MusicService _musicService;
        public MusicModule(LavaNode lavaNode)
        {
            _lavaNode = lavaNode;
            _musicService = new MusicService(_lavaNode);
        }
        [Command("join")]
        public async Task JoinAsync()
        {
            if (_lavaNode.HasPlayer(Context.Guild))
            {
                await Context.Channel.SendMessageAsync(null,false,TemplateMessage("`I'm already connected to a voice channel!`"));
                return;
            }

            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null)
            {
                await Context.Channel.SendMessageAsync(null,false,TemplateMessage("`You must be connected to a voice channel!`"));
                return;
            }

            try
            {
                await _lavaNode.JoinAsync(voiceState.VoiceChannel, Context.Channel as ITextChannel);
                await Context.Channel.SendMessageAsync(null,false,TemplateMessage($"`Joined in {voiceState.VoiceChannel.Name}!`"));
            }
            catch (Exception exception)
            {
                await Context.Channel.SendMessageAsync(exception.Message);
            }
        }
        [Command("leave")]
        public async Task LeaveAsync() => await _musicService.LeaveAsync(Context.Guild);
        [Command("play")]
        [Alias("resume")]
        public async Task PlayAsync([Remainder] string query = null)
        {
            if (!_lavaNode.HasPlayer(Context.Guild)) await JoinAsync();
            await _musicService.PlayAsync(query, Context.Guild);
        }
        [Command("pause")]
        public async Task PauseAsync() => await _musicService.PauseAsync(Context.Guild);
        [Command("stop")]
        public async Task StopAsync() => await _musicService.StopAsync(Context.Guild);
        [Command("next")]
        public async Task NextAsync() => await _musicService.NextAsync(Context.Guild);
        [Command("list")]
        public async Task ListAsync() => await _musicService.ListAsync(Context.Guild);
        [Command("volume")]
        public async Task VolumeAsync(ushort volume) => await _musicService.SetVolumeAsync(volume, Context.Guild);
    }
}
