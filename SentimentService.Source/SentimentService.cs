using InjectorMicroService;
using Newtonsoft.Json;
using PlayerService_2._0;
using RosterLib;
using RosterLib.Helpers;
using SentimentService.Source.Helpers;
using SentimentService.Source.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WikiPages;


namespace SentimentService.Source
{
    public class SentimentService : ISentimentService
    {
        public string Season { get; set; }
        public List<Posture> Postures { get; set; }
        public List<PlayerRank> Ranks { get; set; }
        public INflPlayerService PlayerService { get; set; }

        public string DropboxFolder { get; set; }

        public SentimentService(
            string season,
            string dropboxFolder)
        {
            Season = season;
            DropboxFolder = dropboxFolder;
            PlayerService = new NflPlayerService(
                $"{DropboxFolder}/csv/PlayerCsv-{season}.csv");
        }

        public string Version => "1.0.0";
        public string PosturesFilePath() =>

            $"{DropboxFolder}/JSON/Postures-{Season}.json";

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
            var csvFile = $"{DropboxFolder}/csv/PlayerCsv-{season}.csv";
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

        public AdpPerf AdpPerfForPlayer(
            PerfIdentifier identifier) =>

                AdpPerfForPlayer(
                        identifier.PlayerId,
                        identifier.Season,
                        identifier.Position);


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

        public bool SetSentimentForPlayer(
            PerfIdentifier context)
        {
            var stemfolder = StemFolder("players");

            var mi = new MarkdownInjector(
                stemfolder);

            var playerName = GetPlayerName(context);

            if (string.IsNullOrEmpty(playerName))
            {
                Console.WriteLine(
                    $@"Could not find player name for id {context.PlayerId}");
                return false;
            }
            var targetFile = $"{playerName}.md";
            var fullTargetFile = $"{stemfolder}{targetFile}";
            // may have to create a new page here  
            if (!File.Exists(fullTargetFile))
            {
                var fileOk = CreateMarkdownFile(
                    fullTargetFile,
                    playerName);
                if (!fileOk)
                {
                    Console.WriteLine(
                        $@"Could not create file for player {playerName} at path {fullTargetFile}");
                    return false;
                }
            }

            // doesnt see the "[] style tags
            var result = mi.UpdateProperty(
                propertyName: "sentiment",
                newValue: $"{context.Record.Value.Wins}-{context.Record.Value.Losses}",
                targetFile: targetFile);

            mi.AddPropertyTag(
                "nfl-player",
                targetFile);
            if (context.Record.Value.Wins > 0)
                mi.AddPropertyTag(
                    $"{Season}-hype",
                    targetFile);
            if (context.Record.Value.Losses > 0)
                mi.AddPropertyTag(
                    $"{Season}-fade",
                    targetFile);

            var md = SentimentsHelper.FormatSentiments(
                context.Sentiments);

            if (string.IsNullOrEmpty(md))
                return !string.IsNullOrEmpty(result);

            if ( ! mi.ContainsTag(
                targetFile,
                $"postures-{Season}"))
            {
                mi.AppendTag(
                    targetFile,
                    $"postures-{Season}",
                    $"[[Season {Season}]]");
            }
            mi.InjectMarkdown(
                targetFile,
                $"postures-{Season}",
                md);

            return !string.IsNullOrEmpty(result);
        }

        private bool CreateMarkdownFile(
            string fileName,
            string playerName)
        {
            //  impure file io
            var md = MarkdownTemplate(
                playerName,
                Season);

            bool result;
            try
            {
                using (StreamWriter outputFile = new StreamWriter(
                    fileName))
                {
                    outputFile.WriteLine(md);
                }
                result = true;
            }
            catch (Exception)
            {
                result = false;
            }
            return result;
        }

        private static string MarkdownTemplate(
            string playerName,
            string season) =>

                new WikiPage()
                    .AddTags(new string[] { "nfl-player" })
                    .AddHeading(playerName)
                    .AddBlankLine()
                    .AddHeading($"[[Season {season}]]", 2)
                    .AddBlankLine()
                    .AddLine(StartTag($"postures-{season}"))
                    .AddLine(EndTag($"postures-{season}"))
                    .AddBlankLine()
                    .AddLine(StartTag($"projection-{season}"))
                    .AddLine(EndTag($"projection-{season}"))
                    .AddBlankLine()
                    .AddLine(StartTag($"gamelog-{season}"))
                    .AddLine(EndTag($"gamelog-{season}"))
                    .AddBlankLine()
                    .AddHeading("Career Stats", 2)
                    .AddBlankLine()
                    .AddLine(StartTag("report"))
                    .AddLine(EndTag($"report"))
                    .PageContents();

        private string GetPlayerName(PerfIdentifier context)
        {
            string playerName;
            if (string.IsNullOrEmpty(context.PlayerName))
                playerName = PlayerName(context.PlayerId);
            else
                playerName = context.PlayerName;
            return playerName;
        }

        private string StemFolder(string subFolder) =>
        
            new StringBuilder()
                .Append(DropboxFolder)
                .Append(FolderHelper.GetObsidianNflStemFolder())
                .Append(subFolder)
                .Append("\\")
                .ToString();

        private string PlayerName(string playerId) =>
             PlayerService
                .Search(p => p.ID == playerId)
                .Select(p => p.Name)
                .FirstOrDefault() ?? string.Empty;

        private static string StartTag(string tagName) =>

            new StringBuilder()
                .Append("{")
                .Append(tagName)
                .Append("}")
                .ToString();

        private static string EndTag(string tagName) =>

            new StringBuilder()
                .Append("{/")
                .Append(tagName)
                .Append("}")
                .ToString();
    }
}
