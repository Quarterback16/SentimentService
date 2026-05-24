using RosterLib;
using System.Collections.Generic;

namespace SentimentService.Source.Models
{
    public class PerfIdentifier
    {
        public int Season { get; set; }
        public string Position { get; set; }
        public string PlayerId { get; set; }
        public string PlayerName { get; set; }
        public KeyValuePair<string, WinLossRecord> Record { get; set; }
        public List<Posture> Sentiments { get; set; }
    }
}
