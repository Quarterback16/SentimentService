namespace SentimentService.Source.Models
{
    public class PerfAgainstAdp
    {
        public string PlayerId { get; set; }
        public string Name { get; set; }
        public string Position { get; set; } = string.Empty;
        public int ActualRank { get; set; }
        public int AdpRank { get; set; }
        public decimal TotFp { get; set; }

        public int Perf => AdpRank - ActualRank;

        public override string ToString() =>
            $"{Name} ({Position}) - Actual Rank: {ActualRank}, ADP Rank: {AdpRank}, FP: {TotFp:0.0}, Perf: {Perf:+0;-0}";
    }
}
