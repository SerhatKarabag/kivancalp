using Kivancalp.Gameplay.Application;
using Kivancalp.Gameplay.Configuration;
using NUnit.Framework;

namespace Kivancalp.Gameplay.Tests
{
    [TestFixture]
    public sealed class GameSessionScoringTests
    {
        private const int MatchScore = 100;
        private const int MismatchPenalty = 25;
        private const int ComboBonusStep = 15;

        private GameSessionScoring CreateScoring()
        {
            return new GameSessionScoring(new ScoringConfig(MatchScore, MismatchPenalty, ComboBonusStep));
        }

        [Test]
        public void Reset_SetsAllCountersToZero()
        {
            var scoring = CreateScoring();
            scoring.RecordMatch();
            scoring.RecordMatch();

            scoring.Reset(6);

            Assert.AreEqual(0, scoring.Score);
            Assert.AreEqual(0, scoring.Turns);
            Assert.AreEqual(0, scoring.Matches);
            Assert.AreEqual(0, scoring.Combo);
            Assert.AreEqual(6, scoring.TotalPairs);
            Assert.IsFalse(scoring.IsCompleted);
        }

        [Test]
        public void RecordMatch_IncreasesScoreByMatchScorePlusComboBonus()
        {
            var scoring = CreateScoring();
            scoring.Reset(6);

            scoring.RecordMatch();
            Assert.AreEqual(MatchScore, scoring.Score, "First match: no combo bonus");

            scoring.RecordMatch();
            Assert.AreEqual(MatchScore + (MatchScore + ComboBonusStep), scoring.Score, "Second match: combo=2, bonus = 1*ComboBonusStep");
        }

        [Test]
        public void RecordMatch_IncreasesComboSequentially()
        {
            var scoring = CreateScoring();
            scoring.Reset(6);

            scoring.RecordMatch();
            Assert.AreEqual(1, scoring.Combo);

            scoring.RecordMatch();
            Assert.AreEqual(2, scoring.Combo);

            scoring.RecordMatch();
            Assert.AreEqual(3, scoring.Combo);
        }

        [Test]
        public void RecordMismatch_ResetsComboToZero()
        {
            var scoring = CreateScoring();
            scoring.Reset(6);

            scoring.RecordMatch();
            scoring.RecordMatch();
            Assert.AreEqual(2, scoring.Combo);

            scoring.RecordMismatch();
            Assert.AreEqual(0, scoring.Combo);
        }

        [Test]
        public void RecordMismatch_ScoreDoesNotGoBelowZero()
        {
            var scoring = CreateScoring();
            scoring.Reset(6);

            scoring.RecordMismatch();

            Assert.AreEqual(0, scoring.Score);
        }

        [Test]
        public void RecordMismatch_AppliesPenalty()
        {
            var scoring = CreateScoring();
            scoring.Reset(6);

            scoring.RecordMatch();
            int scoreAfterMatch = scoring.Score;

            scoring.RecordMismatch();

            Assert.AreEqual(scoreAfterMatch - MismatchPenalty, scoring.Score);
        }

        [Test]
        public void IsCompleted_WhenMatchesEqualsTotalPairs()
        {
            var scoring = CreateScoring();
            scoring.Reset(2);

            scoring.RecordMatch();
            Assert.IsFalse(scoring.IsCompleted);

            scoring.RecordMatch();
            Assert.IsTrue(scoring.IsCompleted);
        }

        [Test]
        public void Restore_SetsAllFieldsCorrectly()
        {
            var scoring = CreateScoring();

            scoring.Restore(score: 500, turns: 10, matches: 5, combo: 3, totalPairs: 8, completed: false);

            Assert.AreEqual(500, scoring.Score);
            Assert.AreEqual(10, scoring.Turns);
            Assert.AreEqual(5, scoring.Matches);
            Assert.AreEqual(3, scoring.Combo);
            Assert.AreEqual(8, scoring.TotalPairs);
            Assert.IsFalse(scoring.IsCompleted);
        }

        [Test]
        public void GetStats_ReturnsCorrectSnapshot()
        {
            var scoring = CreateScoring();
            scoring.Reset(4);
            scoring.RecordMatch();

            var stats = scoring.GetStats();

            Assert.AreEqual(MatchScore, stats.Score);
            Assert.AreEqual(1, stats.Turns);
            Assert.AreEqual(1, stats.Matches);
            Assert.AreEqual(1, stats.Combo);
            Assert.AreEqual(4, stats.TotalPairs);
        }
    }
}
