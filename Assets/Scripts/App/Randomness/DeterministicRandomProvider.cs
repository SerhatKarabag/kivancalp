using System;
using Kivancalp.Gameplay.Interfaces;

namespace Kivancalp.Infrastructure.Randomness
{
    public sealed class DeterministicRandomProvider : IRandomProvider
    {
        private readonly int _baseSeed;
        private Random _random;

        public DeterministicRandomProvider(int seed)
        {
            _baseSeed = seed;
            _random = new Random(seed);
        }

        public void Reseed()
        {
            _random = new Random(_baseSeed ^ Environment.TickCount);
        }

        public int NextInt(int minInclusive, int maxExclusive)
        {
            return _random.Next(minInclusive, maxExclusive);
        }

        public void Shuffle(int[] values, int count)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            for (int index = count - 1; index > 0; index -= 1)
            {
                int swapIndex = _random.Next(0, index + 1);
                int value = values[index];
                values[index] = values[swapIndex];
                values[swapIndex] = value;
            }
        }
    }
}
