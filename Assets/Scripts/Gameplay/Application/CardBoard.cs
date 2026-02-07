using System;
using Kivancalp.Gameplay.Interfaces;
using Kivancalp.Gameplay.Models;

namespace Kivancalp.Gameplay.Application
{
    internal sealed class CardBoard
    {
        private readonly int[] _faceIds;
        private readonly CardState[] _cardStates;

        private int _cardCount;

        public CardBoard(int maxCardCount)
        {
            if (maxCardCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxCardCount));
            }

            _faceIds = new int[maxCardCount];
            _cardStates = new CardState[maxCardCount];
        }

        public int CardCount => _cardCount;

        public void Reset(int cardCount, IRandomProvider randomProvider)
        {
            if (randomProvider == null)
            {
                throw new ArgumentNullException(nameof(randomProvider));
            }

            _cardCount = cardCount;

            int faceId = 0;

            for (int cardIndex = 0; cardIndex < _cardCount; cardIndex += 2)
            {
                _faceIds[cardIndex] = faceId;
                _faceIds[cardIndex + 1] = faceId;
                faceId += 1;
            }

            randomProvider.Shuffle(_faceIds, _cardCount);

            for (int cardIndex = 0; cardIndex < _cardCount; cardIndex += 1)
            {
                _cardStates[cardIndex] = CardState.FaceDown;
            }
        }

        public void Restore(int[] faceIds, CardState[] cardStates, int cardCount)
        {
            _cardCount = cardCount;
            Array.Copy(faceIds, 0, _faceIds, 0, cardCount);
            Array.Copy(cardStates, 0, _cardStates, 0, cardCount);
        }

        public CardSnapshot GetSnapshot(int cardIndex)
        {
            if (cardIndex < 0 || cardIndex >= _cardCount)
            {
                throw new ArgumentOutOfRangeException(nameof(cardIndex));
            }

            return new CardSnapshot(cardIndex, _faceIds[cardIndex], _cardStates[cardIndex]);
        }

        public CardState GetState(int cardIndex)
        {
            return _cardStates[cardIndex];
        }

        public void SetState(int cardIndex, CardState state)
        {
            _cardStates[cardIndex] = state;
        }

        public int GetFaceId(int cardIndex)
        {
            return _faceIds[cardIndex];
        }

        public void CopyStateTo(int[] faceIds, CardState[] cardStates, int count)
        {
            Array.Copy(_faceIds, 0, faceIds, 0, count);
            Array.Copy(_cardStates, 0, cardStates, 0, count);
        }
    }
}
