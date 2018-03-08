using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordStats.Data
{
    public class ChannelStats
    {
        private DiscordChannel _channel;

        private ChannelStats() { }

        public ChannelStats(DiscordChannel channel)
        {
            _channel = channel;
            Id = channel.Id;
            Name = channel.Name;
        }

        public ulong Id { get; set; }
        public string Name { get; set; }
        public int Messages { get; set; }
        private int DaysSinceCreate => (int)Math.Ceiling((DateTime.Now - _channel.CreationTimestamp.DateTime).TotalDays);
        public int AvgMessagesPerDay => Messages != 0 ? (int)(Messages / DaysSinceCreate) : 0;

    }
}
