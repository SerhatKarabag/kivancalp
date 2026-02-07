using System;
using Kivancalp.Core;
using Kivancalp.Gameplay.Configuration;
using Kivancalp.Gameplay.Interfaces;
using Kivancalp.Gameplay.Models;

namespace Kivancalp.Gameplay.Application
{
    public sealed class GameSession : IGameSession, IGameSessionLifecycle
    {
        private readonly IRandomProvider _randomProvider;
        private readonly IGamePersistence _persistence;
        private readonly IGameAudio _audio;
        private readonly IGameLogger _logger;
        private readonly CardBoard _board;
        private readonly LayoutNavigator _layoutNavigator;
        private readonly GameSessionScoring _scoring;
        private readonly GameSessionSaveScheduler _saveScheduler;
        private readonly PairMatchingProcessor _pairProcessor;
        private readonly Func<bool> _persistNowCallback;
        private readonly PersistenceBuffers _persistenceBuffers;

        private int _pendingPairFirstCardIndex = -1;
        private float _elapsedTime;
        private bool _audioDisabled;

        public GameSession(GameConfig config, IRandomProvider randomProvider, IGamePersistence persistence, IGameAudio audio, IGameLogger logger)
        {
            Config = config ?? throw new ArgumentNullException(nameof(config));
            _randomProvider = randomProvider ?? throw new ArgumentNullException(nameof(randomProvider));
            _persistence = persistence ?? throw new ArgumentNullException(nameof(persistence));
            _audio = audio ?? throw new ArgumentNullException(nameof(audio));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            int maxCardCount = Config.GetMaxCardCount();

            _board = new CardBoard(maxCardCount);
            _layoutNavigator = new LayoutNavigator(Config);
            _scoring = new GameSessionScoring(Config.Scoring);
            _saveScheduler = new GameSessionSaveScheduler(Config.SaveDebounceSeconds);
            _persistNowCallback = PersistNow;
            _persistenceBuffers = new PersistenceBuffers(maxCardCount);

            _pairProcessor = new PairMatchingProcessor(
                maxCardCount,
                Config.MismatchRevealSeconds,
                _board.GetState,
                _board.GetFaceId,
                OnPairResolved,
                OnCardHidden);
        }

        public event Action<BoardChangedEvent> BoardChanged;

        public event Action<CardStateChangedEvent> CardStateChanged;

        public event Action<PairResolvedEvent> PairResolved;

        public event Action<GameStatsChangedEvent> StatsChanged;

        public event Action<GameCompletedEvent> GameCompleted;

        public GameConfig Config { get; }

        public BoardLayoutConfig CurrentLayout => _layoutNavigator.CurrentLayout;

        public int CurrentLayoutIndex => _layoutNavigator.CurrentLayoutIndex;

        public int CardCount => _board.CardCount;

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

            if (_scoring.IsCompleted)
            {
                RaiseEvent(GameCompleted, new GameCompletedEvent(_scoring.GetStats()));
            }
        }

        public void StartNewGame(LayoutId layoutId)
        {
            int layoutIndex = _layoutNavigator.ResolveLayoutIndex(layoutId);

            _randomProvider.Reseed();
            _layoutNavigator.SetLayout(layoutIndex);
            _board.Reset(_layoutNavigator.CurrentLayout.CardCount, _randomProvider);

            _scoring.Reset(_layoutNavigator.CurrentLayout.PairCount);
            _pendingPairFirstCardIndex = -1;
            _elapsedTime = 0f;

            _pairProcessor.Clear();
            _saveScheduler.Reset();

            PublishBoardChanged();
            PublishStatsChanged();

            MarkDirty();
            ForceSave();
        }

        public void SwitchLayoutByOffset(int offset)
        {
            int targetIndex = _layoutNavigator.CalculateOffsetIndex(offset);

            if (targetIndex == _layoutNavigator.CurrentLayoutIndex && Config.LayoutCount > 0)
            {
                return;
            }

            StartNewGame(Config.GetLayoutByIndex(targetIndex).Id);
        }

        public bool TryFlipCard(int cardIndex)
        {
            if (_scoring.IsCompleted)
            {
                return false;
            }

            if (cardIndex < 0 || cardIndex >= _board.CardCount)
            {
                return false;
            }

            if (_board.GetState(cardIndex) != CardState.FaceDown)
            {
                return false;
            }

            _board.SetState(cardIndex, CardState.FaceUp);
            RaiseEvent(CardStateChanged, new CardStateChangedEvent(cardIndex, CardState.FaceUp, CardStateChangeReason.PlayerFlip));
            PlayAudio(SoundEffectType.Flip);

            if (_pendingPairFirstCardIndex < 0)
            {
                _pendingPairFirstCardIndex = cardIndex;
            }
            else
            {
                _pairProcessor.Enqueue(
                    _pendingPairFirstCardIndex,
                    cardIndex,
                    _elapsedTime + Config.CompareDelaySeconds);

                _pendingPairFirstCardIndex = -1;
            }

            MarkDirty();
            return true;
        }

        public CardSnapshot GetCardSnapshot(int cardIndex)
        {
            return _board.GetSnapshot(cardIndex);
        }

        public GameStats GetStats()
        {
            return _scoring.GetStats();
        }

        public void Tick(float deltaTime)
        {
            if (deltaTime < 0f)
            {
                deltaTime = 0f;
            }

            _elapsedTime += deltaTime;

            _pairProcessor.Tick(_elapsedTime);
            _saveScheduler.Tick(deltaTime, _persistNowCallback);
        }

        public void ForceSave()
        {
            _saveScheduler.ForceSave(_persistNowCallback);
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

        private void OnPairResolved(PairResult result)
        {
            if (result.IsMatch)
            {
                _scoring.RecordMatch();

                _board.SetState(result.First, CardState.Matched);
                _board.SetState(result.Second, CardState.Matched);

                RaiseEvent(CardStateChanged, new CardStateChangedEvent(result.First, CardState.Matched, CardStateChangeReason.Matched));
                RaiseEvent(CardStateChanged, new CardStateChangedEvent(result.Second, CardState.Matched, CardStateChangeReason.Matched));

                PlayAudio(SoundEffectType.Match);

                if (_scoring.IsCompleted)
                {
                    PlayAudio(SoundEffectType.GameOver);
                }
            }
            else
            {
                _scoring.RecordMismatch();
                PlayAudio(SoundEffectType.Mismatch);
            }

            RaiseEvent(PairResolved, new PairResolvedEvent(result.First, result.Second, result.IsMatch));
            PublishStatsChanged();

            if (_scoring.IsCompleted)
            {
                RaiseEvent(GameCompleted, new GameCompletedEvent(_scoring.GetStats()));
            }

            MarkDirty();
        }

        private void OnCardHidden(int cardIndex)
        {
            _board.SetState(cardIndex, CardState.FaceDown);
            RaiseEvent(CardStateChanged, new CardStateChangedEvent(cardIndex, CardState.FaceDown, CardStateChangeReason.AutoHide));
            MarkDirty();
        }

        private bool TryRestore(PersistedGameState persistedState)
        {
            if (!GameSessionPersistenceMapper.TryMapFromPersisted(Config, persistedState, out GameSessionPersistenceMapper.RestoredState restoredState))
            {
                return false;
            }

            _layoutNavigator.SetLayout(restoredState.LayoutIndex);
            _board.Restore(restoredState.FaceIds, restoredState.CardStates, _layoutNavigator.CurrentLayout.CardCount);

            _scoring.Restore(
                restoredState.Score,
                restoredState.Turns,
                restoredState.Matches,
                restoredState.Combo,
                _layoutNavigator.CurrentLayout.PairCount,
                restoredState.IsCompleted);

            _pendingPairFirstCardIndex = -1;
            _elapsedTime = 0f;

            _pairProcessor.Clear();
            _saveScheduler.Reset();

            return true;
        }

        private bool PersistNow()
        {
            int cardCount = _board.CardCount;
            _board.CopyStateTo(_persistenceBuffers.FaceIds, _persistenceBuffers.CardStates, cardCount);

            PersistedGameState persistedState = GameSessionPersistenceMapper.WritePersistedState(
                _persistenceBuffers.CachedState,
                _layoutNavigator.CurrentLayout.Id,
                _scoring.Score,
                _scoring.Turns,
                _scoring.Matches,
                _scoring.Combo,
                _persistenceBuffers.FaceIds,
                _persistenceBuffers.CardStates,
                cardCount);

            try
            {
                return _persistence.Save(persistedState);
            }
            catch (Exception exception)
            {
                _logger.Error("Unexpected exception while persisting game state.", exception);
                return false;
            }
        }

        private void PublishBoardChanged()
        {
            RaiseEvent(BoardChanged, new BoardChangedEvent(_layoutNavigator.CurrentLayout, _layoutNavigator.CurrentLayoutIndex, _board.CardCount));
        }

        private void PublishStatsChanged()
        {
            RaiseEvent(StatsChanged, new GameStatsChangedEvent(_scoring.GetStats()));
        }

        private void MarkDirty()
        {
            _saveScheduler.MarkDirty(_persistNowCallback);
        }

        private void PlayAudio(SoundEffectType effectType)
        {
            if (_audioDisabled)
            {
                return;
            }

            try
            {
                _audio.Play(effectType);
            }
            catch (Exception exception)
            {
                _audioDisabled = true;
                _logger.Error("Audio playback failed. Audio disabled for this session.", exception);
            }
        }

        private void RaiseEvent<T>(Action<T> handler, T args)
        {
            if (handler == null)
            {
                return;
            }

            try
            {
                handler(args);
            }
            catch (Exception exception)
            {
                _logger.Error("Event subscriber threw an exception.", exception);
            }
        }

        private sealed class PersistenceBuffers
        {
            public readonly int[] FaceIds;
            public readonly CardState[] CardStates;
            public readonly PersistedGameState CachedState;

            public PersistenceBuffers(int maxCardCount)
            {
                FaceIds = new int[maxCardCount];
                CardStates = new CardState[maxCardCount];
                CachedState = new PersistedGameState();
            }
        }
    }
}
