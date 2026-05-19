using SentimentService.Source.Models;
using System;
using System.Linq;
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
    }
}
