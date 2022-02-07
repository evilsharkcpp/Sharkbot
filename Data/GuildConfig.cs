using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharkBot.Data
{
    public class GuildConfig
    {
        public string Prefix { get; set; }
        public ulong StdRole { get; set; }
        public string[] WrongWords { get; set; }
        public ulong MutedRoleId { get; set; }
    }
}
