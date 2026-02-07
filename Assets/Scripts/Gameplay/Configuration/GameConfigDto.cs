using System;

namespace Kivancalp.Gameplay.Configuration
{
    [Serializable]
    public sealed class GameConfigDto
    {
        [Serializable]
        public sealed class LayoutDto
        {
            public int id;
            public string name;
            public int rows;
            public int columns;
            public float spacing;
            public float padding;
        }

        [Serializable]
        public sealed class ScoreDto
        {
            public int matchScore = 100;
            public int mismatchPenalty = 25;
            public int comboBonusStep = 15;
        }

        public int defaultLayoutId;
        public LayoutDto[] layouts;
        public ScoreDto score;
        public float flipDurationSeconds;
        public float compareDelaySeconds;
        public float mismatchRevealSeconds;
        public float saveDebounceSeconds;
        public int randomSeed;
    }
}
