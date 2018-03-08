using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordStats.Data
{
    public class MemberStats
    {
        private DiscordMember _member;

        private MemberStats() { }

        public MemberStats(DiscordMember member)
        {
            _member = member;
            Id = member.Id;
        }

        public ulong Id { get; set; }

        public string Username => $"{_member.Username}#{_member.Discriminator}";

        public int SentMessages { get; set; }

        public int Mentions { get; set; }

        public DateTimeOffset LastMessage { get; set; }

        private int DaysSinceJoin => (int)Math.Ceiling((DateTime.Now - _member.JoinedAt.DateTime).TotalDays);

        public int AvgMessagesPerDay => SentMessages != 0 ? (int)(SentMessages / DaysSinceJoin) : 0;
    }
}
