using Microsoft.VisualStudio.TestTools.UnitTesting;
using SentimentService.Source;
using System;
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
                "2026");

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
            Top7("RB");
        }

        [TestMethod]
        public void SentimentServiceKnowsActualQbRanksForSeason()
        {
            Top7("QB");
        }

        [TestMethod]
        public void SentimentServiceKnowsActualWrRanksForSeason()
        {
            Top7("WR");
        }

        [TestMethod]
        public void SentimentServiceKnowsActualTeRanksForSeason()
        {
            Top7("TE");
        }

        private void Top7(string posOfInterest)
        {
            var ranks = _sut?.ActualRanksForSeason(
                2026,
                posOfInterest);
            Assert.IsTrue(ranks?.Any());
            Console.WriteLine($"Number of {posOfInterest} ranks loaded: {ranks?.Count()}");
            for (int i = 0; i < 7; i++)
            {
                Console.WriteLine(
                    $"{posOfInterest}{i + 1:0#}: {ranks[i].Name,-20} {ranks[i].TotFp:0.00}");
            }
        }
    }
}
