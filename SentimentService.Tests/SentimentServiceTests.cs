using Microsoft.VisualStudio.TestTools.UnitTesting;
using RosterLib;
using SentimentService.Source;
using SentimentService.Source.Helpers;
using SentimentService.Source.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SentimentService.Tests
{
    [TestClass]
    public class SentimentServiceTests
    {
        #region  Sut Initialisation

        private ISentimentService _sut;

        [TestInitialize]
        public void TestInitialize()
        {
            _sut = SystemUnderTest();
        }

        private static ISentimentService SystemUnderTest() =>
            new Source.SentimentService(
                "2026",
                "d:/dropbox/");

        #endregion

        [TestMethod]
        public void SentimentServiceInstantiatesOk()
        {
            Assert.IsNotNull(_sut);
        }

        [TestMethod]
        public void SentimentServiceKnowsVersionNumber()
        {
            Assert.AreEqual("1.0.0", _sut?.Version);
        }

        [TestMethod]
        public void SentimentServiceCanFindPosturesFile()
        {
            var postures = _sut?.PosturesFilePath();
            Console.WriteLine(postures);
            Assert.IsFalse(string.IsNullOrEmpty(postures));
        }

        [TestMethod]
        public void SentimentServiceCanLoadPostures()
        {
            var postures = _sut?.LoadPostures();
            Assert.IsTrue(postures?.Any());
            Console.WriteLine($"Number of postures loaded: {postures?.Count()}");
        }

        [TestMethod]
        public void SentimentServiceKnowsActualRbRanksForSeason()
        {
            Top7("RB", 2025);
        }

        [TestMethod]
        public void SentimentServiceKnowsActualQbRanksForSeason()
        {
            Top7("QB", 2025);
        }

        [TestMethod]
        public void SentimentServiceKnowsActualWrRanksForSeason()
        {
            Top7("WR", 2025);
        }

        [TestMethod]
        public void SentimentServiceKnowsActualTeRanksForSeason()
        {
            Top7("TE", 2025);
        }

        private void Top7(
            string posOfInterest,
            int season)
        {
            var ranks = _sut?.ActualRanksForSeason(
                season,
                posOfInterest);
            Assert.IsTrue(ranks?.Any());
            Console.WriteLine($"Season: {season}");
            Console.WriteLine($"Number of {posOfInterest} ranks loaded: {ranks?.Count()}");
            for (int i = 0; i < 7; i++)
            {
                Console.WriteLine(
                    $"{posOfInterest}{i + 1:0#}: {ranks[i].Name,-20} {ranks[i].TotFp:0.00}");
            }
        }

        [TestMethod]
        public void SentimentServiceCanShowQbPerfAgainstAdp()
        {
            PerfAgainstAdp("QB");
        }

        [TestMethod]
        public void SentimentServiceCanShowRbPerfAgainstAdp()
        {
            PerfAgainstAdp("RB");
        }

        [TestMethod]
        public void SentimentServiceCanShowWrPerfAgainstAdp()
        {
            PerfAgainstAdp("WR");
        }

        [TestMethod]
        public void SentimentServiceCanShowTePerfAgainstAdp()
        {
            PerfAgainstAdp("TE");
        }

        private void PerfAgainstAdp(string posOfInterest)
        {
            var sc = new SentimentsContext
            {
                Season = _sut?.Season,
                Postures = _sut?.LoadPostures(),
                Ranks = _sut?.ActualRanksForSeason(
                    Int32.Parse(_sut?.Season) - 1,
                    posOfInterest),
                PlayerService = _sut?.PlayerService
            };
            var performance = SentimentsHelper.PerformanceAgainstAdp(
                sc,
                posOfInterest);
            Assert.IsFalse(string.IsNullOrEmpty(performance));
            Console.WriteLine(performance);
        }

        [TestMethod]
        public void SS_KnowsAdamsOverPerformedAdp()
        {
            var context = new PerfIdentifier
            {
                Season = 2025,
                Position = "WR",
                PlayerId = "ADAMDA01"
            };
            var result = _sut?.AdpPerfForPlayer(context);
            Assert.IsNotNull(result);
            Assert.AreEqual(AdpPerf.Over, result);
        }

        [TestMethod]
        public void SS_KnowsAdamsPlayerRank()
        {
            var posOfInterest = "WR";
            var result = _sut?.PlayerRankForPlayer(
                "ADAMDA01",
                2025,
                posOfInterest);
            Assert.IsNotNull(result);
            Console.WriteLine(
                SentimentsHelper.AdpPerfSummary(
                    posOfInterest,
                    result));
        }

        [TestMethod]
        public void SS_CanUpdateSentiments()
        {
            _sut?.LoadPostures();
            Assert.IsTrue(_sut?.Postures?.Any());

            //Amalgamate the postures by player
            var postDict = new Dictionary<string, WinLossRecord>();
            foreach (var p in _sut?.Postures)
            {
                if (!postDict.ContainsKey(p.Player))
                {
                    postDict[p.Player] = new WinLossRecord(0, 0, 0);
                }
                if (p.PostureFlag == 1)
                {
                    postDict[p.Player].Wins++;
                }
                else
                {
                    postDict[p.Player].Losses++;
                }
            }
            Dictionary<string, WinLossRecord> sortedPlayers = 
                postDict
                    .OrderByDescending(kvp => kvp.Value.Clip())
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            var updates = 0;
            foreach (KeyValuePair<string, WinLossRecord> kvp in sortedPlayers)
            {
                Console.WriteLine(
                    $"{kvp.Key}: {kvp.Value.Wins}-{kvp.Value.Losses}");

                var perfId =new PerfIdentifier
                {
                    PlayerName = RemoveSquareBrackets(kvp.Key),
                };
                _sut?.SetSentimentForPlayer(
                    perfId,
                    $"{kvp.Value.Wins}-{kvp.Value.Losses}");
                updates++;
            }
            Console.WriteLine($"Total updates: {updates}");
        }

        private static string RemoveSquareBrackets(string input) =>
        
            (input == null) 
                ? string.Empty
                : input.Replace("[", string.Empty)
                       .Replace("]", string.Empty);
        
    }
}
