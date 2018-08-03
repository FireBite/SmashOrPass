using System;
using System.Collections.Generic;
using System.Text;

namespace SmashOrPass.Data
{
    public class UserEntry
    {
        public ulong Id { get; set; }
        public string Name { get; set; }
        public ushort Smashes { get; set; }
        public ushort Passes { get; set; }
        public string Url { get; set; }
        public List<ulong> RatedBy { get; set; } = new List<ulong>();
    }
}
