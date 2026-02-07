namespace Kivancalp.Gameplay.Models
{
    public readonly struct GameStats
    {
        public GameStats(int score, int turns, int matches, int combo, int totalPairs)
        {
            Score = score;
            Turns = turns;
            Matches = matches;
            Combo = combo;
            TotalPairs = totalPairs;
        }

        public int Score { get; }

        public int Turns { get; }

        public int Matches { get; }

        public int Combo { get; }

        public int TotalPairs { get; }
    }
}
