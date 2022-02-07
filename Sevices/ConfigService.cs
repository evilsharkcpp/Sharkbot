using SharkBot.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharkBot.Sevices
{
    public class ConfigService
    {
        public Dictionary<ulong, GuildSetup> guildSetups;
        public BotSetup botSetup;
        public ConfigService()
        {
            guildSetups = new Dictionary<ulong, GuildSetup>();
            botSetup = new BotSetup();
            botSetup.InitAsync();
        }
    }
}
