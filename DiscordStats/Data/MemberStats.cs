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
        private int _totalMessages;

        private MemberStats() { }

        public MemberStats(DiscordMember member, int totalMessages)
        {
            _member = member;
            _totalMessages = totalMessages;
            Id = member.Id;
        }

        public ulong Id { get; set; }

        public string Username => $"{_member.Username}#{_member.Discriminator}";

        public int SentMessages { get; set; }

        public double PercentageOfMessages => Math.Round(((double)SentMessages / (double)_totalMessages) * 100D, 2);

        public int Mentions { get; set; }

        public DateTimeOffset LastMessage { get; set; }

        public DateTimeOffset JoinedDiscord => _member.CreationTimestamp;
        public DateTimeOffset JoinedServer => _member.JoinedAt;

        private int DaysSinceJoin => (int)Math.Ceiling((DateTime.Now - _member.JoinedAt.DateTime).TotalDays);

        public int AvgMessagesPerDay => SentMessages != 0 ? (int)(SentMessages / DaysSinceJoin) : 0;
    }
}
