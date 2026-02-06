namespace Kivancalp.Gameplay.Domain
{
    public readonly struct CardSnapshot
    {
        public CardSnapshot(int cardIndex, int faceId, CardState state)
        {
            CardIndex = cardIndex;
            FaceId = faceId;
            State = state;
        }

        public int CardIndex { get; }

        public int FaceId { get; }

        public CardState State { get; }
    }
}
