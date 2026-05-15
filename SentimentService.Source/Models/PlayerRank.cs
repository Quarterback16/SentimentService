namespace SentimentService.Source.Models
{
    public class PlayerRank
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Pos { get; set; }
        public int AdpRank { get; set; }
        public int ActualRank { get; set; }
        public decimal TotFp { get; set; }
    }
}
