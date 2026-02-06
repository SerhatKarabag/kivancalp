using Kivancalp.Gameplay.Configuration;
using Kivancalp.Gameplay.Domain;

namespace Kivancalp.Gameplay.Application
{
    public enum CardStateChangeReason : byte
    {
        PlayerFlip = 0,
        AutoHide = 1,
        Matched = 2,
        LoadRestore = 3,
    }

    public readonly struct BoardChangedEvent
    {
        public BoardChangedEvent(BoardLayoutConfig layout, int layoutIndex, int cardCount)
        {
            Layout = layout;
            LayoutIndex = layoutIndex;
            CardCount = cardCount;
        }

        public BoardLayoutConfig Layout { get; }

        public int LayoutIndex { get; }

        public int CardCount { get; }
    }

    public readonly struct CardStateChangedEvent
    {
        public CardStateChangedEvent(int cardIndex, CardState state, CardStateChangeReason reason)
        {
            CardIndex = cardIndex;
            State = state;
            Reason = reason;
        }

        public int CardIndex { get; }

        public CardState State { get; }

        public CardStateChangeReason Reason { get; }
    }

    public readonly struct PairResolvedEvent
    {
        public PairResolvedEvent(int firstCardIndex, int secondCardIndex, bool isMatch)
        {
            FirstCardIndex = firstCardIndex;
            SecondCardIndex = secondCardIndex;
            IsMatch = isMatch;
        }

        public int FirstCardIndex { get; }

        public int SecondCardIndex { get; }

        public bool IsMatch { get; }
    }

    public readonly struct GameStatsChangedEvent
    {
        public GameStatsChangedEvent(GameStats stats)
        {
            Stats = stats;
        }

        public GameStats Stats { get; }
    }

    public readonly struct GameCompletedEvent
    {
        public GameCompletedEvent(GameStats stats)
        {
            Stats = stats;
        }

        public GameStats Stats { get; }
    }
}
