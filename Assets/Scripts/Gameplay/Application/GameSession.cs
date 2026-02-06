using System;
using Kivancalp.Core.Logging;
using Kivancalp.Gameplay.Configuration;
using Kivancalp.Gameplay.Contracts;
using Kivancalp.Gameplay.Domain;

namespace Kivancalp.Gameplay.Application
{
    public sealed class GameSession : IGameSession
    {
        private struct PendingPair
        {
            public int First;
            public int Second;
            public float ExecuteAt;
        }

        private readonly IRandomProvider _randomProvider;
        private readonly IGamePersistence _persistence;
        private readonly IGameAudio _audio;
        private readonly IGameLogger _logger;
        private readonly int[] _faceIds;
        private readonly CardState[] _cardStates;
        private readonly PendingPair[] _compareQueue;
        private readonly PendingPair[] _hideQueue;

        private int _compareQueueHead;
        private int _compareQueueTail;
        private int _compareQueueCount;
        private int _hideQueueHead;
        private int _hideQueueTail;
        private int _hideQueueCount;

        private int _pendingPairFirstCardIndex = -1;
        private int _cardCount;
        private int _currentLayoutIndex;
        private BoardLayoutConfig _currentLayout;

        private int _score;
        private int _turns;
        private int _matches;
        private int _combo;
        private int _totalPairs;

        private float _elapsedTime;
        private float _saveTimer;
        private bool _dirty;
        private bool _completed;

        public GameSession(GameConfig config, IRandomProvider randomProvider, IGamePersistence persistence, IGameAudio audio, IGameLogger logger)
        {
            Config = config ?? throw new ArgumentNullException(nameof(config));
            _randomProvider = randomProvider ?? throw new ArgumentNullException(nameof(randomProvider));
            _persistence = persistence ?? throw new ArgumentNullException(nameof(persistence));
            _audio = audio ?? throw new ArgumentNullException(nameof(audio));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            int maxCardCount = Config.GetMaxCardCount();
            _faceIds = new int[maxCardCount];
            _cardStates = new CardState[maxCardCount];
            _compareQueue = new PendingPair[maxCardCount];
            _hideQueue = new PendingPair[maxCardCount];

            _currentLayoutIndex = Config.FindLayoutIndex(Config.DefaultLayoutId);

            if (_currentLayoutIndex < 0)
            {
                _currentLayoutIndex = 0;
            }

            _currentLayout = Config.GetLayoutByIndex(_currentLayoutIndex);
        }

        public event Action<BoardChangedEvent> BoardChanged;

        public event Action<CardStateChangedEvent> CardStateChanged;

        public event Action<PairResolvedEvent> PairResolved;

        public event Action<GameStatsChangedEvent> StatsChanged;

        public event Action<GameCompletedEvent> GameCompleted;

        public GameConfig Config { get; }

        public BoardLayoutConfig CurrentLayout => _currentLayout;

        public int CurrentLayoutIndex => _currentLayoutIndex;

        public int CardCount => _cardCount;

        public void Start()
        {
            ReloadFromSave();
        }

        public void ReloadFromSave()
        {
            bool restored = _persistence.TryLoad(out PersistedGameState persistedState) && TryRestore(persistedState);

            if (!restored)
            {
                StartNewGame(Config.DefaultLayoutId);
                return;
            }

            PublishBoardChanged();
            PublishStatsChanged();

            if (_completed)
            {
                GameCompleted?.Invoke(new GameCompletedEvent(GetStats()));
            }
        }

        public void StartNewGame(LayoutId layoutId)
        {
            int layoutIndex = Config.FindLayoutIndex(layoutId);

            if (layoutIndex < 0)
            {
                layoutIndex = Config.FindLayoutIndex(Config.DefaultLayoutId);

                if (layoutIndex < 0)
                {
                    layoutIndex = 0;
                }
            }

            ApplyLayout(layoutIndex);
            GenerateShuffledDeck();

            for (int cardIndex = 0; cardIndex < _cardCount; cardIndex += 1)
            {
                _cardStates[cardIndex] = CardState.FaceDown;
            }

            _score = 0;
            _turns = 0;
            _matches = 0;
            _combo = 0;
            _completed = false;
            _pendingPairFirstCardIndex = -1;

            ClearQueue(ref _compareQueueHead, ref _compareQueueTail, ref _compareQueueCount);
            ClearQueue(ref _hideQueueHead, ref _hideQueueTail, ref _hideQueueCount);

            PublishBoardChanged();
            PublishStatsChanged();

            MarkDirty();
            ForceSave();
        }

        public void SwitchLayoutByOffset(int offset)
        {
            int layoutCount = Config.LayoutCount;

            if (layoutCount <= 0)
            {
                return;
            }

            int targetLayoutIndex = _currentLayoutIndex + offset;

            while (targetLayoutIndex < 0)
            {
                targetLayoutIndex += layoutCount;
            }

            while (targetLayoutIndex >= layoutCount)
            {
                targetLayoutIndex -= layoutCount;
            }

            StartNewGame(Config.GetLayoutByIndex(targetLayoutIndex).Id);
        }

        public bool TryFlipCard(int cardIndex)
        {
            if (_completed || cardIndex < 0 || cardIndex >= _cardCount)
            {
                return false;
            }

            if (_cardStates[cardIndex] != CardState.FaceDown)
            {
                return false;
            }

            _cardStates[cardIndex] = CardState.FaceUp;
            CardStateChanged?.Invoke(new CardStateChangedEvent(cardIndex, CardState.FaceUp, CardStateChangeReason.PlayerFlip));
            _audio.Play(SoundEffectType.Flip);

            if (_pendingPairFirstCardIndex < 0)
            {
                _pendingPairFirstCardIndex = cardIndex;
            }
            else
            {
                Enqueue(
                    _compareQueue,
                    ref _compareQueueHead,
                    ref _compareQueueTail,
                    ref _compareQueueCount,
                    new PendingPair
                    {
                        First = _pendingPairFirstCardIndex,
                        Second = cardIndex,
                        ExecuteAt = _elapsedTime + Config.CompareDelaySeconds,
                    });

                _pendingPairFirstCardIndex = -1;
            }

            MarkDirty();
            return true;
        }

        public CardSnapshot GetCardSnapshot(int cardIndex)
        {
            if (cardIndex < 0 || cardIndex >= _cardCount)
            {
                throw new ArgumentOutOfRangeException(nameof(cardIndex));
            }

            return new CardSnapshot(cardIndex, _faceIds[cardIndex], _cardStates[cardIndex]);
        }

        public GameStats GetStats()
        {
            return new GameStats(_score, _turns, _matches, _combo, _totalPairs);
        }

        public void Tick(float deltaTime)
        {
            if (deltaTime < 0f)
            {
                deltaTime = 0f;
            }

            _elapsedTime += deltaTime;

            ProcessCompareQueue();
            ProcessHideQueue();

            if (_dirty)
            {
                _saveTimer += deltaTime;

                if (_saveTimer >= Config.SaveDebounceSeconds)
                {
                    PersistNow();
                    _dirty = false;
                    _saveTimer = 0f;
                }
            }
        }

        public void ForceSave()
        {
            PersistNow();
            _dirty = false;
            _saveTimer = 0f;
        }

        public void Dispose()
        {
            try
            {
                ForceSave();
            }
            catch (Exception exception)
            {
                _logger.Error("Failed to persist game session during dispose.", exception);
            }
        }

        private void ProcessCompareQueue()
        {
            while (_compareQueueCount > 0)
            {
                PendingPair pair = Peek(_compareQueue, _compareQueueHead);

                if (pair.ExecuteAt > _elapsedTime)
                {
                    break;
                }

                pair = Dequeue(_compareQueue, ref _compareQueueHead, ref _compareQueueTail, ref _compareQueueCount);

                if (_cardStates[pair.First] != CardState.FaceUp || _cardStates[pair.Second] != CardState.FaceUp)
                {
                    continue;
                }

                _turns += 1;
                bool isMatch = _faceIds[pair.First] == _faceIds[pair.Second];

                if (isMatch)
                {
                    _cardStates[pair.First] = CardState.Matched;
                    _cardStates[pair.Second] = CardState.Matched;
                    _matches += 1;
                    _combo += 1;
                    _score += Config.Scoring.MatchScore + ((_combo - 1) * Config.Scoring.ComboBonusStep);

                    CardStateChanged?.Invoke(new CardStateChangedEvent(pair.First, CardState.Matched, CardStateChangeReason.Matched));
                    CardStateChanged?.Invoke(new CardStateChangedEvent(pair.Second, CardState.Matched, CardStateChangeReason.Matched));

                    _audio.Play(SoundEffectType.Match);

                    if (_matches >= _totalPairs)
                    {
                        _completed = true;
                        _audio.Play(SoundEffectType.GameOver);
                    }
                }
                else
                {
                    _combo = 0;
                    _score -= Config.Scoring.MismatchPenalty;

                    if (_score < 0)
                    {
                        _score = 0;
                    }

                    Enqueue(
                        _hideQueue,
                        ref _hideQueueHead,
                        ref _hideQueueTail,
                        ref _hideQueueCount,
                        new PendingPair
                        {
                            First = pair.First,
                            Second = pair.Second,
                            ExecuteAt = _elapsedTime + Config.MismatchRevealSeconds,
                        });

                    _audio.Play(SoundEffectType.Mismatch);
                }

                PairResolved?.Invoke(new PairResolvedEvent(pair.First, pair.Second, isMatch));
                PublishStatsChanged();

                if (_completed)
                {
                    GameCompleted?.Invoke(new GameCompletedEvent(GetStats()));
                }

                MarkDirty();
            }
        }

        private void ProcessHideQueue()
        {
            while (_hideQueueCount > 0)
            {
                PendingPair pair = Peek(_hideQueue, _hideQueueHead);

                if (pair.ExecuteAt > _elapsedTime)
                {
                    break;
                }

                pair = Dequeue(_hideQueue, ref _hideQueueHead, ref _hideQueueTail, ref _hideQueueCount);

                if (_cardStates[pair.First] == CardState.FaceUp)
                {
                    _cardStates[pair.First] = CardState.FaceDown;
                    CardStateChanged?.Invoke(new CardStateChangedEvent(pair.First, CardState.FaceDown, CardStateChangeReason.AutoHide));
                }

                if (_cardStates[pair.Second] == CardState.FaceUp)
                {
                    _cardStates[pair.Second] = CardState.FaceDown;
                    CardStateChanged?.Invoke(new CardStateChangedEvent(pair.Second, CardState.FaceDown, CardStateChangeReason.AutoHide));
                }

                MarkDirty();
            }
        }

        private void GenerateShuffledDeck()
        {
            int faceId = 0;

            for (int cardIndex = 0; cardIndex < _cardCount; cardIndex += 2)
            {
                _faceIds[cardIndex] = faceId;
                _faceIds[cardIndex + 1] = faceId;
                faceId += 1;
            }

            _randomProvider.Shuffle(_faceIds, _cardCount);
        }

        private void ApplyLayout(int layoutIndex)
        {
            _currentLayoutIndex = layoutIndex;
            _currentLayout = Config.GetLayoutByIndex(layoutIndex);
            _cardCount = _currentLayout.CardCount;
            _totalPairs = _currentLayout.PairCount;
            _elapsedTime = 0f;
            _saveTimer = 0f;
        }

        private bool TryRestore(PersistedGameState persistedState)
        {
            if (persistedState == null || persistedState.version != PersistedGameState.CurrentVersion)
            {
                return false;
            }

            if (persistedState.faceIds == null || persistedState.cardStates == null)
            {
                return false;
            }

            int layoutIndex = Config.FindLayoutIndex(new LayoutId(persistedState.layoutId));

            if (layoutIndex < 0)
            {
                return false;
            }

            ApplyLayout(layoutIndex);

            if (persistedState.faceIds.Length != _cardCount || persistedState.cardStates.Length != _cardCount)
            {
                return false;
            }

            int matchedCardCount = 0;

            for (int cardIndex = 0; cardIndex < _cardCount; cardIndex += 1)
            {
                _faceIds[cardIndex] = persistedState.faceIds[cardIndex];
                CardState state = ToCardState(persistedState.cardStates[cardIndex]);

                if (state == CardState.FaceUp)
                {
                    state = CardState.FaceDown;
                }

                _cardStates[cardIndex] = state;

                if (state == CardState.Matched)
                {
                    matchedCardCount += 1;
                }
            }

            _matches = matchedCardCount / 2;
            _turns = persistedState.turns < 0 ? 0 : persistedState.turns;
            _score = persistedState.score < 0 ? 0 : persistedState.score;
            _combo = persistedState.combo < 0 ? 0 : persistedState.combo;
            _completed = _matches >= _totalPairs;
            _pendingPairFirstCardIndex = -1;

            ClearQueue(ref _compareQueueHead, ref _compareQueueTail, ref _compareQueueCount);
            ClearQueue(ref _hideQueueHead, ref _hideQueueTail, ref _hideQueueCount);

            _dirty = false;
            _elapsedTime = 0f;
            _saveTimer = 0f;

            return true;
        }

        private void PersistNow()
        {
            var persistedState = new PersistedGameState
            {
                version = PersistedGameState.CurrentVersion,
                layoutId = _currentLayout.Id.Value,
                score = _score,
                turns = _turns,
                matches = _matches,
                combo = _combo,
                faceIds = new int[_cardCount],
                cardStates = new byte[_cardCount],
            };

            for (int cardIndex = 0; cardIndex < _cardCount; cardIndex += 1)
            {
                persistedState.faceIds[cardIndex] = _faceIds[cardIndex];
                persistedState.cardStates[cardIndex] = (byte)_cardStates[cardIndex];
            }

            _persistence.Save(persistedState);
        }

        private void PublishBoardChanged()
        {
            BoardChanged?.Invoke(new BoardChangedEvent(_currentLayout, _currentLayoutIndex, _cardCount));
        }

        private void PublishStatsChanged()
        {
            StatsChanged?.Invoke(new GameStatsChangedEvent(GetStats()));
        }

        private void MarkDirty()
        {
            _dirty = true;

            if (Config.SaveDebounceSeconds <= 0f)
            {
                PersistNow();
                _dirty = false;
                _saveTimer = 0f;
            }
        }

        private static CardState ToCardState(byte rawState)
        {
            if (rawState == (byte)CardState.FaceDown)
            {
                return CardState.FaceDown;
            }

            if (rawState == (byte)CardState.FaceUp)
            {
                return CardState.FaceUp;
            }

            if (rawState == (byte)CardState.Matched)
            {
                return CardState.Matched;
            }

            return CardState.FaceDown;
        }

        private static void Enqueue(PendingPair[] queue, ref int head, ref int tail, ref int count, PendingPair value)
        {
            if (count >= queue.Length)
            {
                throw new InvalidOperationException("Queue capacity exceeded.");
            }

            queue[tail] = value;
            tail += 1;

            if (tail == queue.Length)
            {
                tail = 0;
            }

            count += 1;
        }

        private static PendingPair Dequeue(PendingPair[] queue, ref int head, ref int tail, ref int count)
        {
            PendingPair value = queue[head];
            head += 1;

            if (head == queue.Length)
            {
                head = 0;
            }

            count -= 1;
            return value;
        }

        private static PendingPair Peek(PendingPair[] queue, int head)
        {
            return queue[head];
        }

        private static void ClearQueue(ref int head, ref int tail, ref int count)
        {
            head = 0;
            tail = 0;
            count = 0;
        }
    }
}
