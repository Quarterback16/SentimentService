using SentimentService.Source.Models;
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
                $"{position} Performance Against ADP for {sc.Season}");

            var table = new WikiTable();
            table.AddColumnRight("#");
            table.AddColumn("Player");
            table.AddColumnRight("Rank");
            table.AddColumnRight("ADP");
            table.AddColumnRight("Diff");
            table.AddColumn("Comments");

            table.AddRows(sc.Ranks.Count);

            var nRow = 0;
            foreach (var p in sc.Ranks)
            {
                table.AddCell(
                    ++nRow,
                    "#",
                    nRow.ToString());
                table.AddCell(
                    nRow,
                    "Player",
                    p.Name);
                table.AddCell(
                    nRow,
                    "Rank",
                    p.ActualRank.ToString());
                table.AddCell(
                    nRow,
                    "ADP",
                    p.AdpRank.ToString());
            }

            page.AddTable(table);

            return page.PageContents();
        }
    }
}
