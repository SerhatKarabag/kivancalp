namespace Kivancalp.Gameplay.Interfaces
{
    public interface IRandomProvider
    {
        void Shuffle(int[] values, int count);

        int NextInt(int minInclusive, int maxExclusive);

        void Reseed();
    }
}
