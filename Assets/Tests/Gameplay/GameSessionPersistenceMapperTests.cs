using Kivancalp.Gameplay.Application;
using Kivancalp.Gameplay.Configuration;
using Kivancalp.Gameplay.Interfaces;
using Kivancalp.Gameplay.Models;
using NUnit.Framework;

namespace Kivancalp.Gameplay.Tests
{
    [TestFixture]
    public sealed class GameSessionPersistenceMapperTests
    {
        private static GameConfig CreateConfig()
        {
            return new GameConfig(
                new[]
                {
                    new BoardLayoutConfig(new LayoutId(1), "2x2", 2, 2, 0f, 0f),
                    new BoardLayoutConfig(new LayoutId(2), "2x4", 2, 4, 0f, 0f),
                },
                new LayoutId(1),
                new ScoringConfig(100, 25, 15),
                0.2f, 0.5f, 1.0f, 2.0f, 42);
        }

        [Test]
        public void WritePersistedState_CopiesAllFieldsCorrectly()
        {
            var target = new PersistedGameState();
            var faceIds = new int[] { 0, 1, 1, 0 };
            var cardStates = new CardState[] { CardState.FaceDown, CardState.FaceUp, CardState.Matched, CardState.FaceDown };

            var result = GameSessionPersistenceMapper.WritePersistedState(
                target, new LayoutId(1), score: 250, turns: 5, matches: 2, combo: 1,
                faceIds, cardStates, cardCount: 4);

            Assert.AreSame(target, result);
            Assert.AreEqual(PersistedGameState.CurrentVersion, result.version);
            Assert.AreEqual(1, result.layoutId);
            Assert.AreEqual(250, result.score);
            Assert.AreEqual(5, result.turns);
            Assert.AreEqual(2, result.matches);
            Assert.AreEqual(1, result.combo);
            Assert.AreEqual(4, result.faceIds.Length);
            Assert.AreEqual(0, result.faceIds[0]);
            Assert.AreEqual(1, result.faceIds[1]);
            Assert.AreEqual((byte)CardState.Matched, result.cardStates[2]);
        }

        [Test]
        public void WritePersistedState_ResizesArraysWhenNeeded()
        {
            var target = new PersistedGameState
            {
                faceIds = new int[2],
                cardStates = new byte[2],
            };

            var faceIds = new int[] { 0, 1, 1, 0 };
            var cardStates = new CardState[] { CardState.FaceDown, CardState.FaceUp, CardState.FaceDown, CardState.FaceDown };

            GameSessionPersistenceMapper.WritePersistedState(
                target, new LayoutId(1), 0, 0, 0, 0, faceIds, cardStates, cardCount: 4);

            Assert.AreEqual(4, target.faceIds.Length);
            Assert.AreEqual(4, target.cardStates.Length);
        }

        [Test]
        public void WritePersistedState_ReusesArrayWhenSameSize()
        {
            var existingFaceIds = new int[4];
            var existingCardStates = new byte[4];
            var target = new PersistedGameState
            {
                faceIds = existingFaceIds,
                cardStates = existingCardStates,
            };

            var faceIds = new int[] { 0, 1, 1, 0 };
            var cardStates = new CardState[] { CardState.FaceDown, CardState.FaceUp, CardState.FaceDown, CardState.FaceDown };

            GameSessionPersistenceMapper.WritePersistedState(
                target, new LayoutId(1), 0, 0, 0, 0, faceIds, cardStates, cardCount: 4);

            Assert.AreSame(existingFaceIds, target.faceIds);
            Assert.AreSame(existingCardStates, target.cardStates);
        }

        [Test]
        public void TryMapFromPersisted_ValidState_ReturnsTrueWithCorrectRestore()
        {
            var config = CreateConfig();
            var state = new PersistedGameState
            {
                version = PersistedGameState.CurrentVersion,
                layoutId = 1,
                score = 300,
                turns = 8,
                matches = 1,
                combo = 2,
                faceIds = new int[] { 0, 1, 1, 0 },
                cardStates = new byte[] { 0, 0, 2, 2 },
            };

            bool success = GameSessionPersistenceMapper.TryMapFromPersisted(config, state, out var restored);

            Assert.IsTrue(success);
            Assert.AreEqual(0, restored.LayoutIndex);
            Assert.AreEqual(300, restored.Score);
            Assert.AreEqual(8, restored.Turns);
            Assert.AreEqual(2, restored.Matches);
            Assert.AreEqual(2, restored.Combo);
            Assert.IsTrue(restored.IsCompleted, "2 matches = all pairs matched for 2x2 layout");
        }

        [Test]
        public void TryMapFromPersisted_WrongVersion_ReturnsFalse()
        {
            var config = CreateConfig();
            var state = new PersistedGameState
            {
                version = 999,
                layoutId = 1,
                faceIds = new int[] { 0, 1, 1, 0 },
                cardStates = new byte[] { 0, 0, 0, 0 },
            };

            bool success = GameSessionPersistenceMapper.TryMapFromPersisted(config, state, out _);

            Assert.IsFalse(success);
        }

        [Test]
        public void TryMapFromPersisted_LayoutNotFound_ReturnsFalse()
        {
            var config = CreateConfig();
            var state = new PersistedGameState
            {
                version = PersistedGameState.CurrentVersion,
                layoutId = 999,
                faceIds = new int[] { 0, 1, 1, 0 },
                cardStates = new byte[] { 0, 0, 0, 0 },
            };

            bool success = GameSessionPersistenceMapper.TryMapFromPersisted(config, state, out _);

            Assert.IsFalse(success);
        }

        [Test]
        public void TryMapFromPersisted_FaceUpCards_ConvertedToFaceDown()
        {
            var config = CreateConfig();
            var state = new PersistedGameState
            {
                version = PersistedGameState.CurrentVersion,
                layoutId = 1,
                score = 0,
                turns = 0,
                combo = 0,
                faceIds = new int[] { 0, 1, 1, 0 },
                cardStates = new byte[] { (byte)CardState.FaceUp, (byte)CardState.FaceUp, 0, 0 },
            };

            GameSessionPersistenceMapper.TryMapFromPersisted(config, state, out var restored);

            Assert.AreEqual(CardState.FaceDown, restored.CardStates[0]);
            Assert.AreEqual(CardState.FaceDown, restored.CardStates[1]);
        }

        [Test]
        public void TryMapFromPersisted_NegativeScore_NormalizedToZero()
        {
            var config = CreateConfig();
            var state = new PersistedGameState
            {
                version = PersistedGameState.CurrentVersion,
                layoutId = 1,
                score = -50,
                turns = 3,
                combo = 0,
                faceIds = new int[] { 0, 1, 1, 0 },
                cardStates = new byte[] { 0, 0, 0, 0 },
            };

            GameSessionPersistenceMapper.TryMapFromPersisted(config, state, out var restored);

            Assert.AreEqual(0, restored.Score);
        }
    }
}
