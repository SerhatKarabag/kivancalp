using System;
using Kivancalp.Gameplay.Models;

namespace Kivancalp.Gameplay.Application
{
    internal sealed class PairMatchingProcessor
    {
        private struct PendingPair
        {
            public int First;
            public int Second;
            public float ExecuteAt;
        }

        private readonly float _mismatchRevealSeconds;
        private readonly Func<int, CardState> _readCardState;
        private readonly Func<int, int> _readFaceId;
        private readonly Action<PairResult> _onPairResolved;
        private readonly Action<int> _onCardHidden;

        private CircularQueue<PendingPair> _compareQueue;
        private CircularQueue<PendingPair> _hideQueue;

        public PairMatchingProcessor(
            int capacity,
            float mismatchRevealSeconds,
            Func<int, CardState> readCardState,
            Func<int, int> readFaceId,
            Action<PairResult> onPairResolved,
            Action<int> onCardHidden)
        {
            _mismatchRevealSeconds = mismatchRevealSeconds;
            _readCardState = readCardState ?? throw new ArgumentNullException(nameof(readCardState));
            _readFaceId = readFaceId ?? throw new ArgumentNullException(nameof(readFaceId));
            _onPairResolved = onPairResolved ?? throw new ArgumentNullException(nameof(onPairResolved));
            _onCardHidden = onCardHidden ?? throw new ArgumentNullException(nameof(onCardHidden));
            _compareQueue = new CircularQueue<PendingPair>(capacity);
            _hideQueue = new CircularQueue<PendingPair>(capacity);
        }

        public void Enqueue(int firstIndex, int secondIndex, float executeAt)
        {
            _compareQueue.Enqueue(new PendingPair
            {
                First = firstIndex,
                Second = secondIndex,
                ExecuteAt = executeAt,
            });
        }

        public void Tick(float elapsedTime)
        {
            ProcessCompareQueue(elapsedTime);
            ProcessHideQueue(elapsedTime);
        }

        public void Clear()
        {
            _compareQueue.Clear();
            _hideQueue.Clear();
        }

        private void ProcessCompareQueue(float elapsedTime)
        {
            while (_compareQueue.Count > 0)
            {
                PendingPair pair = _compareQueue.Peek();

                if (pair.ExecuteAt > elapsedTime)
                {
                    break;
                }

                pair = _compareQueue.Dequeue();

                if (_readCardState(pair.First) != CardState.FaceUp || _readCardState(pair.Second) != CardState.FaceUp)
                {
                    continue;
                }

                bool isMatch = _readFaceId(pair.First) == _readFaceId(pair.Second);

                if (!isMatch)
                {
                    _hideQueue.Enqueue(new PendingPair
                    {
                        First = pair.First,
                        Second = pair.Second,
                        ExecuteAt = elapsedTime + _mismatchRevealSeconds,
                    });
                }

                _onPairResolved(new PairResult(pair.First, pair.Second, isMatch));
            }
        }

        private void ProcessHideQueue(float elapsedTime)
        {
            while (_hideQueue.Count > 0)
            {
                PendingPair pair = _hideQueue.Peek();

                if (pair.ExecuteAt > elapsedTime)
                {
                    break;
                }

                pair = _hideQueue.Dequeue();

                if (_readCardState(pair.First) == CardState.FaceUp)
                {
                    _onCardHidden(pair.First);
                }

                if (_readCardState(pair.Second) == CardState.FaceUp)
                {
                    _onCardHidden(pair.Second);
                }
            }
        }
    }

    internal readonly struct PairResult
    {
        public PairResult(int first, int second, bool isMatch)
        {
            First = first;
            Second = second;
            IsMatch = isMatch;
        }

        public int First { get; }

        public int Second { get; }

        public bool IsMatch { get; }
    }
}
