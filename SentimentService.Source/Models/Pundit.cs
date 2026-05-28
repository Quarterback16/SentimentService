namespace SentimentService.Source.Models
{
    public class Pundit
    {
        public string Name { get; set; }
        public string From { get; set; }
        public string Url { get; set; }
        public int PunditPts { get; set; }
        public int Postures { get; set; }

        public int Wins { get; set; }
        public int Losses { get; set; }

        public int Avg()
        {
            return (Postures > 0) ? PunditPts / Postures : 0;
        }

        public override string ToString() =>
            $"{Name} from {From} - {Postures} postures, {PunditPts} pts, avg {Avg():+0;-0}";
    }
}
