using Newtonsoft.Json;

namespace SentimentService.Source.Models
{
    public class Posture
    {
        public string Pundit { get; set; }
        public string Player { get; set; }
        [JsonProperty("Posture")]
        public int PostureFlag { get; set; } = 0;
        public string Text { get; set; }

        public override string ToString() =>
            $"{Pundit} on {Player}: {(PostureFlag == 1 ? "Hype" : "Fade")} - {Text}";
    }
}
