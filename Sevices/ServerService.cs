using Discord;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SharkBot.Templates;
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
        public void SetMutedRoleAsync(ulong guildId, ulong roleId)
        {
            _configService.guildSetups[guildId].Config.MutedRoleId = roleId;
            SaveConfig(guildId);
        }
        public void AddBadWord(ulong guildId, string badWord)
        {
            var words = new List<string>() {};
            words.AddRange(_configService.guildSetups[guildId].Config.WrongWords);
            words.Add(badWord);
            _configService.guildSetups[guildId].Config.WrongWords = words.ToArray();
            SaveConfig(guildId);
        }
        public async Task GetProfile(IGuild guild, ulong userId, ITextChannel textChannel)
        {
            var guildId = guild.Id;
            var user = await guild.GetUserAsync(userId);
            if (user == null) return;
            DateTime endedTime = DateTime.MaxValue;
            foreach (var item in _configService.guildSetups[guildId].users)
            {
                if(item.Id == user.Id)
                {
                    endedTime = item.TimeEnded;
                }
            }
            await textChannel.SendMessageAsync(embed: GetUserProfile(user,endedTime));

        }
        public void SaveConfig(ulong guildId)
        {
            var json = string.Empty;
            json = JsonConvert.SerializeObject(_configService.guildSetups[guildId].Config, Formatting.Indented);
            File.WriteAllText($"{_configService.guildSetups[guildId].Guildpath}\\{guildId}.json", json, new UTF8Encoding(false));
        }
    }
}
