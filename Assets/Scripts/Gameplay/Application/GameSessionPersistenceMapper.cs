using Kivancalp.Gameplay.Configuration;
using Kivancalp.Gameplay.Interfaces;
using Kivancalp.Gameplay.Models;

namespace Kivancalp.Gameplay.Application
{
    internal static class GameSessionPersistenceMapper
    {
        internal readonly struct RestoredState
        {
            public RestoredState(int layoutIndex, int score, int turns, int matches, int combo, bool isCompleted, int[] faceIds, CardState[] cardStates)
            {
                LayoutIndex = layoutIndex;
                Score = score;
                Turns = turns;
                Matches = matches;
                Combo = combo;
                IsCompleted = isCompleted;
                FaceIds = faceIds;
                CardStates = cardStates;
            }

            public int LayoutIndex { get; }

            public int Score { get; }

            public int Turns { get; }

            public int Matches { get; }

            public int Combo { get; }

            public bool IsCompleted { get; }

            public int[] FaceIds { get; }

            public CardState[] CardStates { get; }
        }

        public static PersistedGameState WritePersistedState(PersistedGameState target, LayoutId layoutId, int score, int turns, int matches, int combo, int[] faceIds, CardState[] cardStates, int cardCount)
        {
            target.version = PersistedGameState.CurrentVersion;
            target.layoutId = layoutId.Value;
            target.score = score;
            target.turns = turns;
            target.matches = matches;
            target.combo = combo;

            if (target.faceIds == null || target.faceIds.Length != cardCount)
            {
                target.faceIds = new int[cardCount];
            }

            if (target.cardStates == null || target.cardStates.Length != cardCount)
            {
                target.cardStates = new byte[cardCount];
            }

            for (int cardIndex = 0; cardIndex < cardCount; cardIndex += 1)
            {
                target.faceIds[cardIndex] = faceIds[cardIndex];
                target.cardStates[cardIndex] = (byte)cardStates[cardIndex];
            }

            return target;
        }

        public static bool TryMapFromPersisted(GameConfig config, PersistedGameState persistedState, out RestoredState restoredState)
        {
            restoredState = default;

            if (persistedState == null || persistedState.version != PersistedGameState.CurrentVersion)
            {
                return false;
            }

            if (persistedState.faceIds == null || persistedState.cardStates == null)
            {
                return false;
            }

            int layoutIndex = config.FindLayoutIndex(new LayoutId(persistedState.layoutId));

            if (layoutIndex < 0)
            {
                return false;
            }

            BoardLayoutConfig layout = config.GetLayoutByIndex(layoutIndex);
            int cardCount = layout.CardCount;

            if (persistedState.faceIds.Length != cardCount || persistedState.cardStates.Length != cardCount)
            {
                return false;
            }

            var restoredFaceIds = new int[cardCount];
            var restoredCardStates = new CardState[cardCount];
            int matchedCardCount = 0;

            for (int cardIndex = 0; cardIndex < cardCount; cardIndex += 1)
            {
                int faceId = persistedState.faceIds[cardIndex];

                if (faceId < 0)
                {
                    return false;
                }

                restoredFaceIds[cardIndex] = faceId;

                CardState cardState = ToCardState(persistedState.cardStates[cardIndex]);

                if (cardState == CardState.FaceUp)
                {
                    cardState = CardState.FaceDown;
                }

                restoredCardStates[cardIndex] = cardState;

                if (cardState == CardState.Matched)
                {
                    matchedCardCount += 1;
                }
            }

            int matches = matchedCardCount / 2;
            restoredState = new RestoredState(
                layoutIndex,
                persistedState.score < 0 ? 0 : persistedState.score,
                persistedState.turns < 0 ? 0 : persistedState.turns,
                matches,
                persistedState.combo < 0 ? 0 : persistedState.combo,
                matches >= layout.PairCount,
                restoredFaceIds,
                restoredCardStates);

            return true;
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
    }
}
