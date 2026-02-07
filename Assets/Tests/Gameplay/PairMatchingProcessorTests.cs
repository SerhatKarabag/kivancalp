using System.Collections.Generic;
using Kivancalp.Gameplay.Application;
using Kivancalp.Gameplay.Models;
using NUnit.Framework;

namespace Kivancalp.Gameplay.Tests
{
    [TestFixture]
    public sealed class PairMatchingProcessorTests
    {
        private Dictionary<int, CardState> _cardStates;
        private Dictionary<int, int> _faceIds;
        private List<PairResult> _resolvedPairs;
        private List<int> _hiddenCards;
        private PairMatchingProcessor _processor;

        [SetUp]
        public void SetUp()
        {
            _cardStates = new Dictionary<int, CardState>();
            _faceIds = new Dictionary<int, int>();
            _resolvedPairs = new List<PairResult>();
            _hiddenCards = new List<int>();

            _processor = new PairMatchingProcessor(
                capacity: 8,
                mismatchRevealSeconds: 1.0f,
                readCardState: index => _cardStates[index],
                readFaceId: index => _faceIds[index],
                onPairResolved: result => _resolvedPairs.Add(result),
                onCardHidden: index => _hiddenCards.Add(index));
        }

        [Test]
        public void Enqueue_Tick_ExecutesAtScheduledTime()
        {
            SetupCard(0, CardState.FaceUp, faceId: 1);
            SetupCard(1, CardState.FaceUp, faceId: 1);

            _processor.Enqueue(0, 1, executeAt: 0.5f);

            _processor.Tick(0.4f);
            Assert.AreEqual(0, _resolvedPairs.Count, "Should not resolve before executeAt");

            _processor.Tick(0.5f);
            Assert.AreEqual(1, _resolvedPairs.Count, "Should resolve at executeAt");
        }

        [Test]
        public void Match_SameFaceId_ReportsIsMatchTrue()
        {
            SetupCard(0, CardState.FaceUp, faceId: 3);
            SetupCard(1, CardState.FaceUp, faceId: 3);

            _processor.Enqueue(0, 1, executeAt: 0f);
            _processor.Tick(0f);

            Assert.AreEqual(1, _resolvedPairs.Count);
            Assert.IsTrue(_resolvedPairs[0].IsMatch);
            Assert.AreEqual(0, _resolvedPairs[0].First);
            Assert.AreEqual(1, _resolvedPairs[0].Second);
        }

        [Test]
        public void Mismatch_DifferentFaceId_ReportsIsMatchFalse()
        {
            SetupCard(0, CardState.FaceUp, faceId: 1);
            SetupCard(1, CardState.FaceUp, faceId: 2);

            _processor.Enqueue(0, 1, executeAt: 0f);
            _processor.Tick(0f);

            Assert.AreEqual(1, _resolvedPairs.Count);
            Assert.IsFalse(_resolvedPairs[0].IsMatch);
        }

        [Test]
        public void Mismatch_HidesCardsAfterRevealDuration()
        {
            SetupCard(0, CardState.FaceUp, faceId: 1);
            SetupCard(1, CardState.FaceUp, faceId: 2);

            _processor.Enqueue(0, 1, executeAt: 0f);
            _processor.Tick(0f);

            Assert.AreEqual(0, _hiddenCards.Count, "Cards should not be hidden yet");

            _processor.Tick(0.5f);
            Assert.AreEqual(0, _hiddenCards.Count, "Still within reveal duration");

            _processor.Tick(1.0f);
            Assert.AreEqual(2, _hiddenCards.Count, "Cards should be hidden after mismatchRevealSeconds");
            Assert.Contains(0, _hiddenCards);
            Assert.Contains(1, _hiddenCards);
        }

        [Test]
        public void Match_DoesNotAddToHideQueue()
        {
            SetupCard(0, CardState.FaceUp, faceId: 5);
            SetupCard(1, CardState.FaceUp, faceId: 5);

            _processor.Enqueue(0, 1, executeAt: 0f);
            _processor.Tick(0f);
            _processor.Tick(10f);

            Assert.AreEqual(0, _hiddenCards.Count);
        }

        [Test]
        public void SkipsCards_NotFaceUp()
        {
            SetupCard(0, CardState.FaceDown, faceId: 1);
            SetupCard(1, CardState.FaceUp, faceId: 1);

            _processor.Enqueue(0, 1, executeAt: 0f);
            _processor.Tick(0f);

            Assert.AreEqual(0, _resolvedPairs.Count, "Should skip pair when card is not FaceUp");
        }

        [Test]
        public void Clear_EmptiesBothQueues()
        {
            SetupCard(0, CardState.FaceUp, faceId: 1);
            SetupCard(1, CardState.FaceUp, faceId: 2);

            _processor.Enqueue(0, 1, executeAt: 0f);
            _processor.Clear();
            _processor.Tick(0f);

            Assert.AreEqual(0, _resolvedPairs.Count);
        }

        [Test]
        public void MultiplePairs_ProcessedInOrder()
        {
            SetupCard(0, CardState.FaceUp, faceId: 1);
            SetupCard(1, CardState.FaceUp, faceId: 1);
            SetupCard(2, CardState.FaceUp, faceId: 2);
            SetupCard(3, CardState.FaceUp, faceId: 2);

            _processor.Enqueue(0, 1, executeAt: 0.1f);
            _processor.Enqueue(2, 3, executeAt: 0.2f);

            _processor.Tick(0.2f);

            Assert.AreEqual(2, _resolvedPairs.Count);
            Assert.AreEqual(0, _resolvedPairs[0].First);
            Assert.AreEqual(2, _resolvedPairs[1].First);
        }

        [Test]
        public void HideQueue_SkipsCards_NoLongerFaceUp()
        {
            SetupCard(0, CardState.FaceUp, faceId: 1);
            SetupCard(1, CardState.FaceUp, faceId: 2);

            _processor.Enqueue(0, 1, executeAt: 0f);
            _processor.Tick(0f);

            _cardStates[0] = CardState.Matched;

            _processor.Tick(1.0f);

            Assert.AreEqual(1, _hiddenCards.Count, "Only the still-FaceUp card should be hidden");
            Assert.AreEqual(1, _hiddenCards[0]);
        }

        private void SetupCard(int index, CardState state, int faceId)
        {
            _cardStates[index] = state;
            _faceIds[index] = faceId;
        }
    }
}
