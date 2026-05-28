using PlayerService_2._0;
using System.Collections.Generic;

namespace SentimentService.Source.Models
{
    public class SentimentsContext
    {
        public string Season { get; set; }
        public string[] Positions { get; set; }
        public List<Posture> Postures { get; set; } = new List<Posture>();
        public List<Pundit> Pundits { get; set; } = new List<Pundit>();
        public List<PlayerRank> Ranks { get; set; } = new List<PlayerRank>();
        public List<PerfAgainstAdp> AllAdp { get; set; } = new List<PerfAgainstAdp>();
        public INflPlayerService PlayerService { get; set; }

        public SentimentsContext() 
        {
            Positions = new[] { "QB", "RB", "WR", "TE" };
        }
    }

   
}
