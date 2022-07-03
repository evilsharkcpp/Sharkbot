using Discord;
using Discord.WebSocket;
using System.Linq;
using System.Threading.Tasks;
using Victoria;
using Victoria.EventArgs;
using Victoria.Responses.Search;
using Victoria.Enums;
using Discord.Commands;
using System;
using static SharkBot.Templates;
using static SharkBot.Utils;
using System.Collections.Generic;
using SharkBot.Data;

namespace SharkBot.Services
{
    public class MusicService
    {
        // Add check null + Exceptions
        private readonly LavaNode _lavaNode;
        static private Dictionary<ulong, Queue<Track>> TrackQueue = new Dictionary<ulong, Queue<Track>>();
        static private Dictionary<ulong, IUserMessage> PlayerMessage = new Dictionary<ulong, IUserMessage>();
        public MusicService(LavaNode lavaNode)
        {
            _lavaNode = lavaNode;
        }
        public void InitEvents()
        {
            _lavaNode.OnTrackEnded += OnTrackEnded;
            _lavaNode.OnTrackStarted += OnTrackStarted;
            _lavaNode.OnTrackException += OnTrackExeption;
        }

        public async Task OnTrackExeption(TrackExceptionEventArgs arg)
        {
            var player = arg.Player;
            var guildId = player.VoiceChannel.GuildId;
            var messagePlayer = PlayerMessage[guildId];
            if (messagePlayer == null) await player.TextChannel.SendMessageAsync(null, false, Player("`Track unsupported`"));
            else await messagePlayer.ModifyAsync(messagePlayer => messagePlayer.Embed = Player("`Track unsupported`"));
            if (TrackQueue[guildId].Count == 0)
            {
                if (messagePlayer == null) await player.TextChannel.SendMessageAsync(null, false, Player("`There are no more tracks in the queue.`"));
                else await messagePlayer.ModifyAsync(messagePlayer => messagePlayer.Embed = Player("`There are no more tracks in the queue.`"));
                await _lavaNode.LeaveAsync(player.VoiceChannel);
                return;
            }
            var track = TrackQueue[guildId].Dequeue();
            if (track.LavaTrack == null)
                track = await GetTrackAsync("", track);

            await player.PlayAsync(track.LavaTrack);
        }

        //Events
        public async Task OnTrackEnded(TrackEndedEventArgs args)
        {
            if (args.Reason != TrackEndReason.Finished) return;
            var player = args.Player;
            var guildId = player.VoiceChannel.GuildId;
            var messagePlayer = PlayerMessage[guildId];
            if (TrackQueue[guildId].Count == 0)
            {
                if (messagePlayer == null) await player.TextChannel.SendMessageAsync(null, false, Player("`There are no more tracks in the queue.`"));
                else await messagePlayer.ModifyAsync(messagePlayer => messagePlayer.Embed = Player("`There are no more tracks in the queue.`"));
                await _lavaNode.LeaveAsync(player.VoiceChannel);
                return;
            }
            var track = TrackQueue[guildId].Dequeue();
            if (track.LavaTrack == null)
                track = await GetTrackAsync("", track);
            await player.PlayAsync(track.LavaTrack);
        }
        public async Task OnTrackStarted(TrackStartEventArgs args)
        {
            var player = args.Player;
            var playerQueue = player.Queue;
            var guildId = player.VoiceChannel.GuildId;
            var messagePlayer = PlayerMessage[guildId];
            var track = player.Track;
            if (messagePlayer == null) PlayerMessage[guildId] = await player.TextChannel.SendMessageAsync(null, false, Player($"Now Playing: `{track.Title}`\n Tracks in queue: {TrackQueue[guildId].Count}"));
            else await messagePlayer.ModifyAsync(messagePlayer => messagePlayer.Embed = Player($"Now Playing: `{track.Title}`\n Tracks in queue: {TrackQueue[guildId].Count}"));
        }
        //Commands
        public async Task ListAsync(IGuild guildId)
        {
            var player = _lavaNode.GetPlayer(guildId);
            string tmp = "";
            int index = 1;
            var queue = TrackQueue[guildId.Id];
            foreach (var item in queue)
            {
                tmp += $"{index++}) {item.Artist} {item.Name}\n";
            }
            await player.TextChannel.SendMessageAsync(null, false, TemplateMessage($"{tmp}Total Count: {player.Queue.Count + TrackQueue[guildId.Id].Count}", "Player"));
        }
        public async Task PlayAsync(string query, IGuild guildId)
        {
            var player = _lavaNode.GetPlayer(guildId);
            if (PlayerMessage.ContainsKey(guildId.Id) == false)
                PlayerMessage.Add(guildId.Id, null);
            var messagePlayer = PlayerMessage[guildId.Id];
            if (TrackQueue.ContainsKey(guildId.Id) == false)
                TrackQueue.Add(guildId.Id, new Queue<Track>());
            if (player.PlayerState == PlayerState.Paused)
            {
                await player.ResumeAsync();
                await player.TextChannel.SendMessageAsync(null, false, TemplateMessage("`Resume playing`", "Player"));
                return;
            }
            var parsed = await ParseUrl(query);
            if (parsed != null)
            {
                var track = await GetTrackAsync("", parsed[0]);
                if (player.PlayerState == PlayerState.Playing || player.PlayerState == PlayerState.Paused)
                {
                    for (var i = 0; i < parsed.Count; i++)
                        if (i == 0) TrackQueue[guildId.Id].Enqueue(track);
                        else TrackQueue[guildId.Id].Enqueue(parsed[i]);
                    await player.TextChannel.SendMessageAsync(null, false, TemplateMessage("`Tracks added in queue`", "Player"));
                }
                else
                {
                    for (var i = 1; i < parsed.Count; i++)
                        TrackQueue[guildId.Id].Enqueue(parsed[i]);
                    await player.PlayAsync(track.LavaTrack);
                }
            }
            else
            {
                var track = await GetTrackAsync(query);
                if (player.PlayerState == PlayerState.Playing || player.PlayerState == PlayerState.Paused)
                {
                    TrackQueue[guildId.Id].Enqueue(track);
                    await player.TextChannel.SendMessageAsync(null, false, TemplateMessage("`Track added in queue`", "Player"));
                }
                else
                {
                    await player.PlayAsync(track.LavaTrack);
                }
            }
        }
        public async Task PauseAsync(IGuild guildId)
        {
            var player = _lavaNode.GetPlayer(guildId);
            if (player.PlayerState == PlayerState.Paused)
            {
                await player.TextChannel.SendMessageAsync(null, false, TemplateMessage("`I am already paused`", "Player"));
                return;
            }
            if (player != null)
                await player.PauseAsync();
            await player.TextChannel.SendMessageAsync(null, false, TemplateMessage("`Music paused`", "Player"));
            return;
        }
        public async Task StopAsync(IGuild guildId)
        {
            var player = _lavaNode.GetPlayer(guildId);
            if (player != null)
                await player.StopAsync();
            await player.TextChannel.SendMessageAsync(null, false, TemplateMessage("`Music Stopped`", "Player"));
        }
        public async Task LeaveAsync(IGuild guildId)
        {
            var player = _lavaNode.GetPlayer(guildId);
            TrackQueue.Remove(guildId.Id);
            await _lavaNode.LeaveAsync(player.VoiceChannel);
        }
        public async Task NextAsync(IGuild guildId)
        {
            var player = _lavaNode.GetPlayer(guildId);
            var messagePlayer = PlayerMessage[guildId.Id];
            if (player != null && TrackQueue[guildId.Id].Count > 0)
            {
                var track = TrackQueue[guildId.Id].Dequeue();
                if (track.LavaTrack == null)
                    track = await GetTrackAsync("", track);
                await player.TextChannel.SendMessageAsync(null, false, TemplateMessage($"`Track` skipped", "Player"));
                await player.PlayAsync(track.LavaTrack);
            }
        }
        public async Task SetVolumeAsync(ushort volume, IGuild guildId)
        {
            var player = _lavaNode.GetPlayer(guildId);
            await player.UpdateVolumeAsync(volume);
            await player.TextChannel.SendMessageAsync(null, false, TemplateMessage($"Volume changed.", "Player"));
        }
        public async Task<Track> GetTrackAsync(string url, Track track = null)
        {
            if (url.Contains("youtube.com") || url.Contains("youtu.be"))
            {
                var response = await _lavaNode.SearchYouTubeAsync(url);
                if (response.Status != SearchStatus.NoMatches && response.Status != SearchStatus.LoadFailed)
                {
                    var tracks = new Track("", response.Tracks.First().Title, response.Tracks.First());
                    return tracks;
                }
            }
            else
            {
                if (url.Contains("soundcloud.com"))
                {
                    var response = await _lavaNode.SearchSoundCloudAsync(url);
                    if (response.Status != SearchStatus.NoMatches && response.Status != SearchStatus.LoadFailed)
                    {
                        var tracks = new Track("", response.Tracks.First().Title, response.Tracks.First());
                        return tracks;
                    }
                }
                else
                {
                    if (track != null)
                    {
                        var response = await _lavaNode.SearchYouTubeAsync($"{track.Artist} {track.Name}");
                        if (response.Status != SearchStatus.NoMatches && response.Status != SearchStatus.LoadFailed)
                        {
                            var tmp = url.Split(';');
                            var tracks = new Track(track.Artist, track.Name, response.Tracks.First());
                            return tracks;
                        }
                    }
                    else
                    {
                        var response = await _lavaNode.SearchYouTubeAsync(url);
                        if (response.Status != SearchStatus.NoMatches && response.Status != SearchStatus.LoadFailed)
                        {
                            var tracks = new Track("", response.Tracks.First().Title, response.Tracks.First());
                            return tracks;
                        }
                    }
                }
            }
            return null;
        }
    }
}