using System;

namespace Kivancalp.Gameplay.Contracts
{
    [Serializable]
    public sealed class PersistedGameState
    {
        public const int CurrentVersion = 1;

        public int version = CurrentVersion;
        public int layoutId;
        public int score;
        public int turns;
        public int matches;
        public int combo;
        public int[] faceIds;
        public byte[] cardStates;
    }
}
