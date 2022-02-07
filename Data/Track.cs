using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Victoria;

namespace SharkBot.Data
{
    public class Track
    {
        public readonly string Artist;
        public readonly string Name;
        public readonly LavaTrack LavaTrack;
        public Track(string artist, string name, LavaTrack lavaTrack)
        {
            Artist = artist;
            Name = name;
            LavaTrack = lavaTrack;
        }
    }
}
