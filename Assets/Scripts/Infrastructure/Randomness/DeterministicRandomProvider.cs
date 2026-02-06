using System;
using Kivancalp.Gameplay.Contracts;

namespace Kivancalp.Infrastructure.Randomness
{
    public sealed class DeterministicRandomProvider : IRandomProvider
    {
        private readonly Random _random;

        public DeterministicRandomProvider(int seed)
        {
            _random = new Random(seed);
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
