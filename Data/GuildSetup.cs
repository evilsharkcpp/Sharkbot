using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharkBot.Data
{
    public class GuildSetup
    {
       public GuildConfig Config { get; set; }
       public List<UserInfo> users { get; set; }
        public string Guildpath { get; set; } = Directory.GetCurrentDirectory() + "\\Guilds";
        public GuildSetup(string path)
        {
            path = Guildpath + "\\" + path;
            var json = string.Empty;
            if(!File.Exists(path))
            {
                json = JsonConvert.SerializeObject(GenerateNewConfig, Formatting.Indented);
                File.WriteAllText(path, json, new UTF8Encoding(false));
                
            }
            json = File.ReadAllText(path, new UTF8Encoding(false));
            Config = JsonConvert.DeserializeObject<GuildConfig>(json);
            users = new List<UserInfo>();
        }

        private GuildConfig GenerateNewConfig => new GuildConfig
        {
            Prefix = "#",
            StdRole = 0,
            WrongWords = new string[] { "Блять", "Пидр"},
            MutedRoleId = 0
        };
    }
}
