using PlayerService_2._0;
using System.Collections.Generic;

namespace SentimentService.Source.Models
{
    public class SentimentsContext
    {
        public string Season { get; set; }
        public List<Posture> Postures { get; set; }
        public List<PlayerRank> Ranks { get; set; }
        public INflPlayerService PlayerService { get; set; }
    }
}
