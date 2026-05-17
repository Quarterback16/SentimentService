using Newtonsoft.Json;
using PlayerService_2._0;
using RosterLib;
using RosterLib.Helpers;
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
        public List<PlayerRank> Ranks { get; set; }
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
            var csvFile = $"d:/dropbox/csv/PlayerCsv-{season}.csv";
            Console.WriteLine($"Using csv file :{csvFile}");
            PlayerService = new NflPlayerService(csvFile);

            bool skipRookies = false;
            if (season.ToString() != Utility.CurrentSeason())
                skipRookies = true;

            Ranks = new List<PlayerRank>();
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
            {
                if (p.RookieYr.StartsWith(Utility.CurrentSeason())
                    && skipRookies)
                        continue;
                
                Ranks.Add(
                    new PlayerRank
                    {
                        ActualRank = ++rank,
                        Id = p.ID,
                        Name = p.Name,
                        Pos = p.Pos,
                        AdpRank = AdpHelper.PosAdpRankFromString(p.PosAdp),
                        TotFp = TotalFp(p)
                    });
            }
            return Ranks;
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

        public AdpPerf AdpPerfForPlayer(
            string playerId,
            int season,
            string position)
        {
            ActualRanksForSeason(
                season,
                position);

            var performance = Ranks
                .Where(r => r.Id == playerId)
                .Select(r => r.AdpRank - r.ActualRank)
                .FirstOrDefault();

            if (performance < -2)
                return AdpPerf.Under;
            else if (performance > 2)
                return AdpPerf.Over;
            else
              return AdpPerf.AsExpected;
        }

        public PlayerRank PlayerRankForPlayer(
            string playerId,
            int season,
            string position)
        {
            ActualRanksForSeason(
                season,
                position);

            return Ranks
                .FirstOrDefault(r => r.Id == playerId);
        }

        public AdpPerf AdpPerfForPlayer(
            AdpPerfIdentifier identifier) =>

                AdpPerfForPlayer(
                        identifier.PlayerId,
                        identifier.Season,
                        identifier.Position);
    }
}
