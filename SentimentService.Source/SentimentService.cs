using Newtonsoft.Json;
using PlayerService_2._0;
using SentimentService.Source.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SentimentService.Source
{
    public class SentimentService : ISentimentService
    {
        public string Season { get; set; }
        public List<Posture> Postures { get; set; }
        public INflPlayerService PlayerService { get; set; }

        public SentimentService(
            string season)
        {
            Season = season;
        }

        public string Version => "1.0.0";
        public string PosturesFilePath() =>

            $"d:/Dropbox/JSON/Postures-{Season}.json";

        public List<Posture> LoadPostures() =>

            LoadPostures(
                PosturesFilePath());

        public List<Posture> LoadPostures(
            string filePath)
        {
            var result = new List<Posture>();
            if (string.IsNullOrEmpty(filePath))
            {
                return result;
            }
            using (var r = new StreamReader(filePath))
            {
                var json = r.ReadToEnd();
                result = JsonConvert.DeserializeObject<List<Posture>>(json);
            }
            Postures = result;
            return result;
        }

        public List<PlayerRank> ActualRanksForSeason(
            int season, 
            string position)
        {
            PlayerService = new NflPlayerService(
                $"d:/dropbox/csv/PlayerCsv-{season - 1}.csv");

            var result = new List<PlayerRank>();
            Func<NflPlayerState, bool> posFn = null;
            switch (position)
            {
                case "QB":
                    posFn = QB;
                    break;
                case "RB":
                    posFn = RB;
                    break;
                case "WR":
                    posFn = WR;
                    break;
                case "TE":
                    posFn = TE;
                    break;
                default:
                    posFn = (NflPlayerState p) => true;
                    break;
            }
            var players = PlayerService
                .Search(posFn)
                .OrderByDescending(p=>TotalFp(p));
            var rank = 0;
            foreach (var p in players)
                result.Add(
                    new PlayerRank 
                    { 
                        ActualRank = ++rank, 
                        Id = p.ID, 
                        Name = p.Name,
                        TotFp = TotalFp(p)
                    });
            return result;
        }

        public static Func<NflPlayerState, bool> RB =>
            (NflPlayerState p) => p.Role != null && (p.Pos.Contains("RB")
            || p.Pos.Contains("HB")) && !QB(p);

        public static Func<NflPlayerState, bool> QB =>
            (NflPlayerState p) => p.Role != null && p.Pos.Contains(nameof(QB));

        public static Func<NflPlayerState, bool> WR =>
            (NflPlayerState p) => p.Role != null && p.Pos.Contains("WR");

        public static Func<NflPlayerState, bool> TE =>
            (NflPlayerState p) => p.Role != null
                && p.Pos.Contains("TE");

        private static decimal TotalFp(NflPlayerState p) =>

            decimal.Parse(p.FP01) 
            + decimal.Parse(p.FP02)
            + decimal.Parse(p.FP03)
            + decimal.Parse(p.FP04) 
            + decimal.Parse(p.FP05)
            + decimal.Parse(p.FP06) 
            + decimal.Parse(p.FP07)
            + decimal.Parse(p.FP08)
            + decimal.Parse(p.FP09)
            + decimal.Parse(p.FP10)
            + decimal.Parse(p.FP11)
            + decimal.Parse(p.FP12)
            + decimal.Parse(p.FP13)
            + decimal.Parse(p.FP14)
            + decimal.Parse(p.FP15)
            + decimal.Parse(p.FP16)
            + decimal.Parse(p.FP17);
    }
}
