using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharkBot.Sevices
{
    public class ServerService
    {
        ConfigService _configService;
        public ServerService(ConfigService configService) => _configService = configService;

        public void SetPrefixAsync(ulong guildId, string NewPrefix)
        {
            _configService.guildSetups[guildId].Config.Prefix = NewPrefix;
            SaveConfig(guildId);
        }
        public void SetStdRoleAsync(ulong guildId, ulong roleId)
        {
            _configService.guildSetups[guildId].Config.StdRole = roleId;
            SaveConfig(guildId);
        }
        public void SaveConfig(ulong guildId)
        {
            var json = string.Empty;
            json = JsonConvert.SerializeObject(_configService.guildSetups[guildId].Config, Formatting.Indented);
            File.WriteAllText($"{_configService.guildSetups[guildId].Guildpath}\\{guildId}.json", json, new UTF8Encoding(false));
        }
    }
}
