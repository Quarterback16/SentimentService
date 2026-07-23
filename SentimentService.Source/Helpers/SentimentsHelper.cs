using SentimentService.Source.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WikiPages;

namespace SentimentService.Source.Helpers
{
    public static class SentimentsHelper
    {
        public static string PerformanceAgainstAdp(
            SentimentsContext sc,
            string position)
        {
            var page = new WikiPage();
            page.AddHeading(
                $"{position} Performance Against ADP for {sc.Season}",
                2);

            var table = new WikiTable();
            table.AddColumnRight("#");
            table.AddColumn("Player");
            table.AddColumnRight("Rank");
            table.AddColumnRight("ADP");
            table.AddColumnRight("Diff");
            table.AddColumnRight("FP");
            table.AddColumn("Comments");

            table.AddRows(sc.Ranks.Count(r => r.TotFp > 0));

            var maxAdp = MaxAdp(sc);
            Console.WriteLine($"Max ADP: {maxAdp}");

            var nRow = 0;
            foreach (var p in sc.Ranks.OrderByDescending(
                p => AdpDiff(p, maxAdp)))
            {
                if (p.TotFp == 0)
                    continue;

                table.AddCell(
                    ++nRow,
                    "#",
                    nRow.ToString());
                table.AddCell(
                    nRow,
                    "Player",
                    $"[[{p.Name}]]");
                table.AddCell(
                    nRow,
                    "Rank",
                    p.ActualRank.ToString());
                table.AddCell(
                    nRow,
                    "ADP",
                    p.AdpRank.ToString());
                table.AddCell(
                    nRow,
                    "FP",
                    p.TotFp.ToString("0.0"));
                table.AddCell(
                    nRow,
                    "Diff",
                    $"{AdpDiff(p,maxAdp):+0;-0}");
            }

            page.AddTable(table);

            return page.PageContents();
        }

        private static int MaxAdp(
            SentimentsContext sc) =>
        
            sc.Ranks
                .DefaultIfEmpty()
                .Max(p => p?.AdpRank ?? 0);        

        public static int AdpDiff(
            PlayerRank p,
            int maxAdp) =>
        
            (p.AdpRank > 0) 
                ? p.AdpRank - p.ActualRank
                : maxAdp - p.ActualRank;

        public static int AdpDiff(PlayerRank rank) =>
        
            rank.AdpRank - rank.ActualRank;
        
        public static string AdpPerfSummary(
            string position, 
            PlayerRank rank) =>

            $"{rank.Name} was drafted at {position}{rank.AdpRank:0#} and finished {position}{rank.ActualRank:0#}, {AdpDiff(rank):+0;-0}.";

        public static string FormatSentiments(
            List<Posture> sentiments)
        {
            var sb = new StringBuilder();
            foreach (var s in sentiments)
            {
                sb.AppendLine(
                    $"- {SentimentIcon(s)} [[{s.Pundit}]] : {s.Text}");
            } 
            return sb.ToString();
        }

        private static string SentimentIcon(Posture s) =>
        
            s.PostureFlag == 1
                ? "✅"
                : "❌";

        public static string PunditsToMarkdown(
            PunditContext pc)
        {
            var page = new WikiPage();
            page.AddHeading(
                $"Pundits for {pc.Season}",
                2);
            var table = new WikiTable();
            table.AddColumn("#");
            table.AddColumn(nameof(Pundit));
            table.AddColumn("Comments");
            table.AddRows(pc.Pundits.Count);

            var nRow = 0;
            foreach (var p in pc.Pundits)
            {
                table.AddCell(
                    ++nRow,
                    nameof(Pundit),
                    $"[[{p.Name}]]");
                table.AddCell(
                    nRow,
                    "#",
                    nRow.ToString());
            }

            page.AddTable(table);
            return page.PageContents();
        }

        public static string BestPunditsToMarkdown(
            SentimentsContext sc)
        {
            var page = new WikiPage();
            page.AddHeading(
                $"Best Pundits for {sc.Season}",
                2);
            var table = new WikiTable();
            table.AddColumnRight("#");
            table.AddColumn("Pundit");
            table.AddColumnRight("Avg");
            table.AddColumnRight("Postures");
            table.AddColumnRight("Wins");
            table.AddColumnRight("Losses");
            table.AddColumnRight("Pts");
            table.AddColumn("Comments");
            table.AddRows(sc.Pundits.Count+1);

            var nRow = 0;
            var nTotPts = 0;
            var nPostures = 0;
            var nWins = 0;
            var nLosses = 0;
            foreach (var p in sc.Pundits
                .OrderByDescending(p => p.Avg()))
            {
                table.AddCell(
                    ++nRow,
                    "Pundit",
                    $"[[{p.Name}]]");
                table.AddCell(
                    nRow,
                    "#",
                    nRow.ToString());
                table.AddCell(
                    nRow,
                    "Pts",
                    $"{p.PunditPts:+0;-0}");
                table.AddCell(
                    nRow,
                    "Postures",
                    $"{p.PostureCount}");
                table.AddCell(
                    nRow,
                    "Avg",
                    $"{p.Avg()}");
                table.AddCell(
                    nRow,
                    "Wins",
                    $"{p.Wins}");
                table.AddCell(
                    nRow,
                    "Losses",
                    $"{p.Losses}");
                nLosses += p.Losses;
                nWins += p.Wins;
                nTotPts += p.PunditPts;
                nPostures += p.PostureCount;
            }
            table.AddCell(
                ++nRow,
                "Pundit",
                "TOTALS");
            table.AddCell(
                nRow,
                "Pts",
                $"{nTotPts:+0;-0}");
            table.AddCell(
                nRow,
                "Wins",
                $"{nWins}");
            table.AddCell(
                nRow,
                "Losses",
                $"{nLosses}");
            page.AddTable(table);
            return page.PageContents();
        }

        public static string PunditPageToMarkdown(
            Pundit pundit,
            string season)
        {
            var page = new WikiPage();
            page.AddTags(
                new string[] 
                {
                    "nfl-pundit" 
                });
            page.AddHeading(pundit.Name);
            page.AddBlankLine();
            page.AddHeading(
                $"[[Season {season}]] postures",
                2);
            page.AddBlankLine();
            page.AddLine($"{{postures-{season}}}");
            page.AddLine($"{{/postures-{season}}}");
            page.AddBlankLine();

            page.AddLine("##");
            page.AddHorizontalRule();

            return page.PageContents();
        }

        public static string FormatPostures(
            List<Posture> postures)
        {
            var sb = new StringBuilder();
            foreach (var p in postures)
            {
                sb.AppendLine(
                    $"- {SentimentIcon(p)} [[{p.Player}]] : {p.Text}");
            }
            return sb.ToString();
        }
    }
}
