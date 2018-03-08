using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordStats
{
    public class StatsConfig 
    {
        public StatsConfig()
        {
            UserMessageCounts = true;
            UserMentionCounts = true;
            ChannelMessageCounts = true;
            MessageContentStats = false;
            BanMeDaddy = false;
        }

        public DiscordGuild Guild { get; set; }
        public DiscordChannel InChannel { get; set; }

        public bool? UserMessageCounts { get; set; }
        public bool? UserMentionCounts { get; set; }
        public bool? ChannelMessageCounts { get; set; }
        public bool? MessageContentStats { get; set; }
        public bool? BanMeDaddy { get; set; }

        public DateTime? BeforeDate { get; set; }
        public DateTime? AfterDate { get; set; }
    }
}
