using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharkBot.Data;
using SpotifyAPI.Web;

namespace SharkBot
{
    public static class Utils
    {
        // Don't get spotify url. Try find tracks by author + name
        public static async Task<List<Track>> ParseUrl(string url)
        {
            if (!url.Contains("spotify.com")) return null;
            var config = SpotifyClientConfig.CreateDefault();
            if (url.Contains("?si="))
                url = url.Remove(url.IndexOf("?si="));
            var request = new ClientCredentialsRequest("fc9db120a4c64091b3211dd8e626cc6b", "fe0fb4c1daa140e8b15c64c10ee037e0");
            var response = await new OAuthClient(config).RequestToken(request);

            var spotify = new SpotifyClient(config.WithToken(response.AccessToken));
            List<Track> result = new List<Track>();
            if (url.Contains("/track/"))
            {
                string str = "/track/";
                string id = url.Substring(url.IndexOf("/track/") + str.Length);
                var track = await spotify.Tracks.Get(id);
                var artists = "";
                foreach (var item in track.Artists)
                    artists += $"{item.Name}, ";
                artists = artists.Remove(artists.Length - 2);
                result.Add(new Track(artists, track.Name, null));
            }
            if (url.Contains("/album/"))
            {
                string str = "/album/";
                string id = url.Substring(url.IndexOf("/album/") + str.Length);
                var album = await spotify.Albums.Get(id);
                var allPages = await spotify.PaginateAll(album.Tracks);
                foreach (var i in allPages)
                {
                    var artists = "";
                    foreach (var item in i.Artists)
                        artists += $"{item.Name}, ";
                    artists = artists.Remove(artists.Length - 2);
                    result.Add(new Track(artists, i.Name, null));
                }
            }
            if (url.Contains("/playlist/"))
            {
                string str = "/playlist/";
                string id = url.Substring(url.LastIndexOf("/playlist/") + str.Length);
                FullPlaylist playlist = await spotify.Playlists.Get(id);
                foreach (var i in playlist.Tracks.Items)
                {
                    if (i.Track is FullTrack track)
                    {
                        var artists = "";
                        foreach (var item in track.Artists)
                            artists += $"{item.Name}, ";
                        artists = artists.Remove(artists.Length - 2);
                        result.Add(new Track(artists, track.Name, null));
                    }
                    if (i.Track is FullEpisode episode)
                    {
                        result.Add(new Track("", episode.Name, null));
                    }
                }
            }
            if (url.Contains("/artist/"))
            {
                string str = "/artist/";
                string id = url.Substring(url.LastIndexOf("/artist/") + str.Length);
                var artist = await spotify.Artists.GetTopTracks(id, new ArtistsTopTracksRequest("RU"));
                foreach (var i in artist.Tracks)
                {
                    var artists = "";
                    foreach (var item in i.Artists)
                        artists += $"{item.Name}, ";
                    artists = artists.Remove(artists.Length - 2);
                    result.Add(new Track(artists, i.Name, null));
                }
            }
            return result;
        }
    }
}
