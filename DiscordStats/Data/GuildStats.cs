using DSharpPlus.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordStats.Data
{
    public class GuildStats
    {
        DiscordGuild _guild;

        private GuildStats() { }

        public GuildStats(DiscordGuild guild)
        {
            _guild = guild;
        }

        [JsonIgnore]
        public DiscordGuild Guild => _guild;

        public int TotalMessages { get; set; }

        public int MessagesAccountedFor { get; set; }

        public int MessagesPerDay => TotalMessages / DaysSinceCreate;

        private int DaysSinceCreate => (int)Math.Ceiling((DateTime.Now - _guild.CreationTimestamp.DateTime).TotalDays);

        public List<ChannelStats> ChannelStats { get; set; } = new List<ChannelStats>();
        public List<MemberStats> MemberStats { get; set; } = new List<MemberStats>();
    }
}
