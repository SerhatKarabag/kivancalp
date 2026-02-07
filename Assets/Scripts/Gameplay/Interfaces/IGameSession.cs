using System;
using Kivancalp.Gameplay.Configuration;
using Kivancalp.Gameplay.Models;

namespace Kivancalp.Gameplay.Interfaces
{
    public interface IGameSession
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

        void StartNewGame(LayoutId layoutId);

        void SwitchLayoutByOffset(int offset);

        bool TryFlipCard(int cardIndex);

        CardSnapshot GetCardSnapshot(int cardIndex);

        GameStats GetStats();
    }
}
