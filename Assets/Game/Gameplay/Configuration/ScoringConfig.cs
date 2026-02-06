namespace Kivancalp.Gameplay.Configuration
{
    public readonly struct ScoringConfig
    {
        public ScoringConfig(int matchScore, int mismatchPenalty, int comboBonusStep)
        {
            MatchScore = matchScore;
            MismatchPenalty = mismatchPenalty;
            ComboBonusStep = comboBonusStep;
        }

        public int MatchScore { get; }

        public int MismatchPenalty { get; }

        public int ComboBonusStep { get; }
    }
}
