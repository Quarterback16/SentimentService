using System.Collections.Generic;

namespace SentimentService.Source
{
    public interface ISentimentService
    {
        string Version { get; }

        List<Models.Posture> LoadPostures();
        List<Models.Posture> LoadPostures(
            string filePath);
        string PosturesFilePath();

        List<Models.PlayerRank> ActualRanksForSeason(
            int season, 
            string position);
    }
}
