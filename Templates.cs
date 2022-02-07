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
