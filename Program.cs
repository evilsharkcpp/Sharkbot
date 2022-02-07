using SharkBot.Sevices;

namespace SharkBot
{
    class Program
    {
        static void Main(string[] args) => new DiscordService().RunBotAsync().GetAwaiter().GetResult();
    }
}