using Kivancalp.Gameplay.Configuration;
using Kivancalp.Gameplay.Models;

namespace Kivancalp.Gameplay.Application
{
    internal sealed class GameSessionScoring
    {
        private readonly ScoringConfig _config;

        private int _score;
        private int _turns;
        private int _matches;
        private int _combo;
        private int _totalPairs;
        private bool _completed;

        public GameSessionScoring(ScoringConfig config)
        {
            _config = config;
        }

        public int Score => _score;

        public int Turns => _turns;

        public int Matches => _matches;

        public int Combo => _combo;

        public int TotalPairs => _totalPairs;

        public bool IsCompleted => _completed;

        public void Reset(int totalPairs)
        {
            _score = 0;
            _turns = 0;
            _matches = 0;
            _combo = 0;
            _totalPairs = totalPairs;
            _completed = false;
        }

        public void Restore(int score, int turns, int matches, int combo, int totalPairs, bool completed)
        {
            _score = score;
            _turns = turns;
            _matches = matches;
            _combo = combo;
            _totalPairs = totalPairs;
            _completed = completed;
        }

        public void RecordMatch()
        {
            _turns += 1;
            _matches += 1;
            _combo += 1;
            _score += _config.MatchScore + ((_combo - 1) * _config.ComboBonusStep);

            if (_matches >= _totalPairs)
            {
                _completed = true;
            }
        }

        public void RecordMismatch()
        {
            _turns += 1;
            _combo = 0;
            _score -= _config.MismatchPenalty;

            if (_score < 0)
            {
                _score = 0;
            }
        }

        public GameStats GetStats()
        {
            return new GameStats(_score, _turns, _matches, _combo, _totalPairs);
        }
    }
}
