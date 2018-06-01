using DSharpPlus;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordStats
{
    public class StatsConfig : INotifyPropertyChanged
    {
        private DiscordGuild _guild;

        public StatsConfig()
        {
            UserMessageCounts = true;
            UserMentionCounts = true;
            ChannelMessageCounts = true;
            MessageContentStats = false;
            BanMeDaddy = false;
            IncludeBannedUsers = false;
        }

        public DiscordGuild Guild
        {
            get => _guild;
            set
            {
                _guild = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanListBans)));
            }
        }

        public DiscordChannel InChannel { get; set; }

        public bool CanListBans => Guild?.IsOwner == true || Guild?.Channels.First().PermissionsFor(Guild.CurrentMember).HasFlag(Permissions.BanMembers) == true;

        public bool? UserMessageCounts { get; set; }
        public bool? UserMentionCounts { get; set; }
        public bool? ChannelMessageCounts { get; set; }
        public bool? MessageContentStats { get; set; }
        public bool? IncludeBannedUsers { get; set; }
        public bool? BanMeDaddy { get; set; }

        public DateTime? BeforeDate { get; set; }
        public DateTime? AfterDate { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
