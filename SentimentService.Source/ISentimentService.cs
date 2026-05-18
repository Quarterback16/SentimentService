using PlayerService_2._0;
using SentimentService.Source.Models;
using System.Collections.Generic;

namespace SentimentService.Source
{
    public interface ISentimentService
    {
        string Version { get; }
        string Season { get; }
        INflPlayerService PlayerService { get; }

        List<Models.Posture> LoadPostures();
        List<Models.Posture> LoadPostures(
            string filePath);

        string PosturesFilePath();

        List<PlayerRank> ActualRanksForSeason(
            int season, 
            string position);

        AdpPerf AdpPerfForPlayer(
            string playerId,
            int season,
            string position);

        AdpPerf AdpPerfForPlayer(
            PerfIdentifier identifier);

        PlayerRank PlayerRankForPlayer(
            string playerId,
            int season,
            string position);

        bool SetSentimentForPlayer(
            PerfIdentifier context, 
            string sentiment);
    }
}
