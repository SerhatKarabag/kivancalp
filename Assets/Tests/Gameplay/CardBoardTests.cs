using System;
using Kivancalp.Gameplay.Application;
using Kivancalp.Gameplay.Interfaces;
using Kivancalp.Gameplay.Models;
using NUnit.Framework;

namespace Kivancalp.Gameplay.Tests
{
    [TestFixture]
    public sealed class CardBoardTests
    {
        [Test]
        public void Reset_CreatesCorrectNumberOfPairs()
        {
            var board = new CardBoard(8);
            board.Reset(8, new IdentityRandomProvider());

            var faceIdCounts = new int[4];

            for (int i = 0; i < 8; i++)
            {
                int faceId = board.GetFaceId(i);
                Assert.IsTrue(faceId >= 0 && faceId < 4, "FaceId should be between 0 and 3");
                faceIdCounts[faceId]++;
            }

            for (int i = 0; i < 4; i++)
            {
                Assert.AreEqual(2, faceIdCounts[i], "Each faceId should appear exactly twice");
            }
        }

        [Test]
        public void Reset_AllCardsFaceDown()
        {
            var board = new CardBoard(6);
            board.Reset(6, new IdentityRandomProvider());

            for (int i = 0; i < 6; i++)
            {
                Assert.AreEqual(CardState.FaceDown, board.GetState(i));
            }
        }

        [Test]
        public void Restore_CopiesStateCorrectly()
        {
            var board = new CardBoard(4);
            var faceIds = new int[] { 0, 1, 1, 0 };
            var cardStates = new CardState[] { CardState.FaceDown, CardState.FaceUp, CardState.Matched, CardState.FaceDown };

            board.Restore(faceIds, cardStates, 4);

            Assert.AreEqual(4, board.CardCount);
            Assert.AreEqual(1, board.GetFaceId(1));
            Assert.AreEqual(CardState.FaceUp, board.GetState(1));
            Assert.AreEqual(CardState.Matched, board.GetState(2));
        }

        [Test]
        public void GetSnapshot_ReturnsCorrectData()
        {
            var board = new CardBoard(4);
            var faceIds = new int[] { 5, 3, 3, 5 };
            var cardStates = new CardState[] { CardState.FaceDown, CardState.FaceUp, CardState.FaceDown, CardState.Matched };
            board.Restore(faceIds, cardStates, 4);

            var snapshot = board.GetSnapshot(1);

            Assert.AreEqual(1, snapshot.CardIndex);
            Assert.AreEqual(3, snapshot.FaceId);
            Assert.AreEqual(CardState.FaceUp, snapshot.State);
        }

        [Test]
        public void GetSnapshot_InvalidIndex_ThrowsArgumentOutOfRangeException()
        {
            var board = new CardBoard(4);
            board.Reset(4, new IdentityRandomProvider());

            Assert.Throws<ArgumentOutOfRangeException>(() => board.GetSnapshot(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => board.GetSnapshot(4));
        }

        [Test]
        public void SetState_GetState_RoundTrip()
        {
            var board = new CardBoard(4);
            board.Reset(4, new IdentityRandomProvider());

            board.SetState(2, CardState.FaceUp);
            Assert.AreEqual(CardState.FaceUp, board.GetState(2));

            board.SetState(2, CardState.Matched);
            Assert.AreEqual(CardState.Matched, board.GetState(2));
        }

        [Test]
        public void CopyStateTo_CopiesCorrectly()
        {
            var board = new CardBoard(4);
            var faceIds = new int[] { 0, 1, 1, 0 };
            var cardStates = new CardState[] { CardState.FaceDown, CardState.FaceUp, CardState.Matched, CardState.FaceDown };
            board.Restore(faceIds, cardStates, 4);

            var outFaceIds = new int[4];
            var outCardStates = new CardState[4];
            board.CopyStateTo(outFaceIds, outCardStates, 4);

            Assert.AreEqual(faceIds, outFaceIds);
            Assert.AreEqual(cardStates, outCardStates);
        }

        [Test]
        public void Constructor_InvalidMaxCardCount_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new CardBoard(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new CardBoard(-1));
        }

        #region Test Doubles

        private sealed class IdentityRandomProvider : IRandomProvider
        {
            public void Shuffle(int[] values, int count) { }
            public int NextInt(int minInclusive, int maxExclusive) => minInclusive;
            public void Reseed() { }
        }

        #endregion
    }
}
