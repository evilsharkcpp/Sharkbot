using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharkBot.Data
{
    public class UserInfo
    {
        public ulong Id { get; set; }
        public ulong ViolationCount { get; set; } = 0;
        public DateTime TimeEnded { get; set; } = DateTime.MaxValue;
        public UserInfo(ulong Id)
        {
            this.Id = Id;
        }
    }
}
