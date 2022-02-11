using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharkBot
{
    public static class Templates
    {
        public static Embed GetUserProfile(IGuildUser user, DateTime time)
        {
            var embedBuilder = new EmbedBuilder();
            embedBuilder.Color = Color.DarkBlue;
            embedBuilder.ThumbnailUrl = user.GetAvatarUrl();
            embedBuilder.AddField("Name", user.Username);
            embedBuilder.AddField("MutedTime", time == DateTime.MaxValue ? "No mute" : time.ToString());
            return embedBuilder.Build();
        }
        public static Embed Player(string text)
        {
            var embedMessage = new EmbedBuilder();
            embedMessage.Color = Color.Blue;
            embedMessage.AddField("Player",text);
            return embedMessage.Build();
        }
        public static Embed TemplateMessage(string botMessage, string reason = "SharkBot", string imgUrl = null)
        {
            var embedMessage = new EmbedBuilder();
            if (imgUrl != null) embedMessage.ThumbnailUrl = imgUrl;
            embedMessage.Color = Color.Blue;
            embedMessage.AddField($"{reason}", $"{botMessage}");
            return embedMessage.Build();

        }
    }
}
