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
        static public Dictionary<ulong, Queue<Track>> queue = new Dictionary<ulong, Queue<Track>>();
        SocketUserMessage messagePlayer;
        public MusicService(LavaNode lavaNode)
        {
            _lavaNode = lavaNode;
        }
        public void InitEvents()
        {
            _lavaNode.OnTrackEnded += OnTrackEnded;
            //_lavaNode.OnTrackStarted += OnTrackStarted;
            _lavaNode.OnTrackException += OnTrackExeption;
        }

        public async Task OnTrackExeption(TrackExceptionEventArgs arg)
        {
            var player = arg.Player;
            var guildId = player.VoiceChannel.GuildId;
            await player.TextChannel.SendMessageAsync(null, false, TemplateMessage("`Track unsupported`", "Player"));
            if (queue[guildId].Count == 0)
            {
                await player.TextChannel.SendMessageAsync(null, false, TemplateMessage("`There are no more tracks in the queue.`"));
                await _lavaNode.LeaveAsync(player.VoiceChannel);
                return;
            }
            var track = queue[guildId].Dequeue();
            if (track.LavaTrack == null)
                track = await GetTrackAsync("", track);

            await player.TextChannel.SendMessageAsync(null, false, TemplateMessage($"Now Playing: `{track.Artist} {track.Name}`\n Tracks in queue: {queue[guildId].Count}", "Player"));
            await player.PlayAsync(track.LavaTrack);
        }

        //Events
        public async Task OnTrackEnded(TrackEndedEventArgs args)
        {
            if (args.Reason != TrackEndReason.Finished) return;
            var player = args.Player;
            var guildId = player.VoiceChannel.GuildId;
            if (queue[guildId].Count == 0)
            {
                await player.TextChannel.SendMessageAsync(null, false, TemplateMessage("`There are no more tracks in the queue.`"));
                await _lavaNode.LeaveAsync(player.VoiceChannel);
                return;
            }
            var track = queue[guildId].Dequeue();
            if (track.LavaTrack == null)
                track = await GetTrackAsync("", track);

            await player.TextChannel.SendMessageAsync(null, false, TemplateMessage($"Now Playing: `{track.Artist} {track.Name}`\n Tracks in queue: {queue[guildId].Count}", "Player"));
            await player.PlayAsync(track.LavaTrack);
        }
        public async Task OnTrackStarted(TrackStartEventArgs args)
        {
            var player = args.Player;
            var playerQueue = player.Queue;
            //await player.TextChannel.SendMessageAsync(null, false, TemplateMessage($"Now Playing: `{args.Track.Title}`\n Tracks in queue: {playerQueue.Count + queue[player.VoiceChannel.GuildId].Count}", "Player"));
        }
        //Commands
        public async Task ListAsync(IGuild guildId)
        {
            var player = _lavaNode.GetPlayer(guildId);
            string tmp = "";
            int index = 1;
            foreach (var item in player.Queue)
            {
                tmp += index++.ToString() + ") " + item.Title + "\n";
            }
            tmp += "...\n";
            await player.TextChannel.SendMessageAsync(null, false, TemplateMessage($"{tmp}Total Count: {player.Queue.Count + queue[guildId.Id].Count}", "Player"));
        }
        public async Task PlayAsync(string query, IGuild guildId)
        {
            var player = _lavaNode.GetPlayer(guildId);
            if (queue.ContainsKey(guildId.Id) == false)
                queue.Add(guildId.Id, new Queue<Track>());
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
                        if (i == 0) queue[guildId.Id].Enqueue(track);
                        else queue[guildId.Id].Enqueue(parsed[i]);
                    await player.TextChannel.SendMessageAsync(null, false, TemplateMessage("`Tracks added in queue`", "Player"));
                }
                else
                {
                    for (var i = 1; i < parsed.Count; i++)
                        queue[guildId.Id].Enqueue(parsed[i]);
                    if (parsed.Count > 1) await player.TextChannel.SendMessageAsync(null, false, TemplateMessage("`Tracks added in queue`", "Player"));
                    await player.TextChannel.SendMessageAsync(null, false, TemplateMessage($"Now Playing: `{track.Artist} {track.Name}`\n Tracks in queue: {queue[guildId.Id].Count}", "Player"));
                    await player.PlayAsync(track.LavaTrack);
                }
            }
            else
            {
                var track = await GetTrackAsync(query);
                if (player.PlayerState == PlayerState.Playing || player.PlayerState == PlayerState.Paused)
                {
                    queue[guildId.Id].Enqueue(track);
                    await player.TextChannel.SendMessageAsync(null, false, TemplateMessage("`Track added in queue`", "Player"));
                }
                else
                {
                    await player.TextChannel.SendMessageAsync(null, false, TemplateMessage($"Now Playing: `{track.Artist} {track.Name}`\n Tracks in queue: {queue[guildId.Id].Count}", "Player"));
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
            queue.Remove(guildId.Id);
            await _lavaNode.LeaveAsync(player.VoiceChannel);
        }
        public async Task NextAsync(IGuild guildId)
        {
            var player = _lavaNode.GetPlayer(guildId);
            if (player != null && queue[guildId.Id].Count > 0)
            {
                var track = queue[guildId.Id].Dequeue();
                if (track.LavaTrack == null)
                    track = await GetTrackAsync("", track);
                await player.TextChannel.SendMessageAsync(null, false, TemplateMessage($"`Track` skipped", "Player"));
                await player.TextChannel.SendMessageAsync(null, false, TemplateMessage($"Now Playing: `{track.Artist} {track.Name}`\n Tracks in queue: {queue[guildId.Id].Count}", "Player"));
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