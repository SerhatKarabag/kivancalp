using Kivancalp.Gameplay.Configuration;
using UnityEngine;

namespace Kivancalp.Infrastructure.Config
{
    [CreateAssetMenu(menuName = "Kivancalp/Game Config", fileName = "game_config")]
    public sealed class GameConfigAsset : ScriptableObject
    {
        public const string ResourcePath = "Configs/game_config";

        public int defaultLayoutId = 1;
        public GameConfigDto.LayoutDto[] layouts = CreateDefaultLayouts();
        public GameConfigDto.ScoreDto score = new GameConfigDto.ScoreDto();
        public float flipDurationSeconds = 0.16f;
        public float compareDelaySeconds = 0.10f;
        public float mismatchRevealSeconds = 0.45f;
        public float saveDebounceSeconds = 0.20f;
        public int randomSeed = 20260206;

        public static GameConfigAsset LoadOrCreateRuntimeDefault()
        {
            GameConfigAsset configAsset = Resources.Load<GameConfigAsset>(ResourcePath);

            if (configAsset == null)
            {
                configAsset = CreateInstance<GameConfigAsset>();
            }

            configAsset.EnsureInitialized();
            return configAsset;
        }

        public GameConfigDto ToDto()
        {
            EnsureInitialized();

            return new GameConfigDto
            {
                defaultLayoutId = defaultLayoutId,
                layouts = layouts,
                score = score,
                flipDurationSeconds = flipDurationSeconds,
                compareDelaySeconds = compareDelaySeconds,
                mismatchRevealSeconds = mismatchRevealSeconds,
                saveDebounceSeconds = saveDebounceSeconds,
                randomSeed = randomSeed,
            };
        }

        public void EnsureInitialized()
        {
            if (layouts == null || layouts.Length == 0)
            {
                layouts = CreateDefaultLayouts();
            }

            if (score == null)
            {
                score = new GameConfigDto.ScoreDto();
            }

            if (flipDurationSeconds < 0f)
            {
                flipDurationSeconds = 0f;
            }

            if (compareDelaySeconds < 0f)
            {
                compareDelaySeconds = 0f;
            }

            if (mismatchRevealSeconds < 0f)
            {
                mismatchRevealSeconds = 0f;
            }

            if (saveDebounceSeconds < 0f)
            {
                saveDebounceSeconds = 0f;
            }
        }

        private static GameConfigDto.LayoutDto[] CreateDefaultLayouts()
        {
            return new[]
            {
                new GameConfigDto.LayoutDto { id = 0, name = "2x2", rows = 2, columns = 2, spacing = 16f, padding = 24f },
                new GameConfigDto.LayoutDto { id = 1, name = "2x3", rows = 2, columns = 3, spacing = 16f, padding = 24f },
                new GameConfigDto.LayoutDto { id = 2, name = "2x4", rows = 2, columns = 4, spacing = 16f, padding = 24f },
                new GameConfigDto.LayoutDto { id = 3, name = "3x4", rows = 3, columns = 4, spacing = 16f, padding = 24f },
                new GameConfigDto.LayoutDto { id = 4, name = "4x4", rows = 4, columns = 4, spacing = 14f, padding = 20f },
                new GameConfigDto.LayoutDto { id = 5, name = "4x5", rows = 4, columns = 5, spacing = 14f, padding = 20f },
                new GameConfigDto.LayoutDto { id = 6, name = "4x6", rows = 4, columns = 6, spacing = 12f, padding = 20f },
                new GameConfigDto.LayoutDto { id = 7, name = "5x6", rows = 5, columns = 6, spacing = 12f, padding = 20f },
            };
        }
    }
}
