using System;
using Kivancalp.Core.Lifecycle;
using Kivancalp.Gameplay.Application;
using Kivancalp.Gameplay.Configuration;
using Kivancalp.Gameplay.Domain;

namespace Kivancalp.Gameplay.Contracts
{
    public interface IGameSession : ITickable, IDisposable
    {
        event Action<BoardChangedEvent> BoardChanged;

        event Action<CardStateChangedEvent> CardStateChanged;

        event Action<PairResolvedEvent> PairResolved;

        event Action<GameStatsChangedEvent> StatsChanged;

        event Action<GameCompletedEvent> GameCompleted;

        GameConfig Config { get; }

        BoardLayoutConfig CurrentLayout { get; }

        int CurrentLayoutIndex { get; }

        int CardCount { get; }

        void Start();

        void StartNewGame(LayoutId layoutId);

        void SwitchLayoutByOffset(int offset);

        bool TryFlipCard(int cardIndex);

        CardSnapshot GetCardSnapshot(int cardIndex);

        GameStats GetStats();

        void ForceSave();

        void ReloadFromSave();
    }
}
