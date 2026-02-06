using System;

namespace Kivancalp.Gameplay.Configuration
{
    public sealed class GameConfig
    {
        private readonly BoardLayoutConfig[] _layouts;

        public GameConfig(BoardLayoutConfig[] layouts, LayoutId defaultLayoutId, ScoringConfig scoring, float flipDurationSeconds, float compareDelaySeconds, float mismatchRevealSeconds, float saveDebounceSeconds, int randomSeed)
        {
            if (layouts == null)
            {
                throw new ArgumentNullException(nameof(layouts));
            }

            if (layouts.Length == 0)
            {
                throw new ArgumentException("At least one layout is required.", nameof(layouts));
            }

            _layouts = layouts;
            DefaultLayoutId = defaultLayoutId;
            Scoring = scoring;
            FlipDurationSeconds = flipDurationSeconds;
            CompareDelaySeconds = compareDelaySeconds;
            MismatchRevealSeconds = mismatchRevealSeconds;
            SaveDebounceSeconds = saveDebounceSeconds;
            RandomSeed = randomSeed;
        }

        public LayoutId DefaultLayoutId { get; }

        public ScoringConfig Scoring { get; }

        public float FlipDurationSeconds { get; }

        public float CompareDelaySeconds { get; }

        public float MismatchRevealSeconds { get; }

        public float SaveDebounceSeconds { get; }

        public int RandomSeed { get; }

        public int LayoutCount => _layouts.Length;

        public BoardLayoutConfig GetLayoutByIndex(int index)
        {
            return _layouts[index];
        }

        public int FindLayoutIndex(LayoutId layoutId)
        {
            for (int index = 0; index < _layouts.Length; index += 1)
            {
                if (_layouts[index].Id == layoutId)
                {
                    return index;
                }
            }

            return -1;
        }

        public BoardLayoutConfig GetLayout(LayoutId layoutId)
        {
            int index = FindLayoutIndex(layoutId);

            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(layoutId), "Layout not found: " + layoutId.Value);
            }

            return _layouts[index];
        }

        public int GetMaxCardCount()
        {
            int max = 0;

            for (int index = 0; index < _layouts.Length; index += 1)
            {
                int cardCount = _layouts[index].CardCount;

                if (cardCount > max)
                {
                    max = cardCount;
                }
            }

            return max;
        }
    }
}
