using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharkBot.Sevices;

namespace SharkBot.Data
{
    public class BotSetup
    {
        public BotConfig Config { get; set; }
        public static string BotPath { get; set; } = "config.json";
        public void InitAsync()
        {
            var json = string.Empty;
            if (File.Exists(BotPath))
            {
                json = File.ReadAllText(BotPath, new UTF8Encoding(false));
                Config = JsonConvert.DeserializeObject<BotConfig>(json);
            }
            else
            {
                json = JsonConvert.SerializeObject(GenerateNewConfig, Formatting.Indented);
                File.WriteAllText(BotPath, json, new UTF8Encoding(false));
                Console.WriteLine("Please check config.json");
            }
        }

        private BotConfig GenerateNewConfig => new BotConfig
        {
            Token = "Put Token here",
            Prefix = "#",
            GameStatus = "Dev bot"
        };
    }
}
