using System;
using Kivancalp.Core.Logging;
using Kivancalp.Gameplay.Configuration;
using Kivancalp.Gameplay.Contracts;
using UnityEngine;

namespace Kivancalp.Infrastructure.Config
{
    public sealed class ResourcesGameConfigProvider : IGameConfigProvider
    {
        private const string ConfigResourcePath = "Game/card_match_config";

        private const string FallbackConfigJson = "{\"defaultLayoutId\":1,\"layouts\":[{\"id\":0,\"name\":\"2x2\",\"rows\":2,\"columns\":2,\"spacing\":16.0,\"padding\":24.0},{\"id\":1,\"name\":\"2x3\",\"rows\":2,\"columns\":3,\"spacing\":16.0,\"padding\":24.0},{\"id\":2,\"name\":\"5x6\",\"rows\":5,\"columns\":6,\"spacing\":12.0,\"padding\":20.0}],\"score\":{\"matchScore\":100,\"mismatchPenalty\":25,\"comboBonusStep\":15},\"flipDurationSeconds\":0.16,\"compareDelaySeconds\":0.10,\"mismatchRevealSeconds\":0.45,\"saveDebounceSeconds\":0.20,\"randomSeed\":20260206}";

        private readonly IGameLogger _logger;
        private GameConfig _cachedConfig;

        public ResourcesGameConfigProvider(IGameLogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public GameConfig Load()
        {
            if (_cachedConfig != null)
            {
                return _cachedConfig;
            }

            string json = FallbackConfigJson;
            TextAsset configAsset = Resources.Load<TextAsset>(ConfigResourcePath);

            if (configAsset != null)
            {
                json = configAsset.text;
            }
            else
            {
                _logger.Warning("Config file not found in Resources. Falling back to built-in defaults.");
            }

            GameConfigDto dto = JsonUtility.FromJson<GameConfigDto>(json);

            if (dto == null)
            {
                throw new InvalidOperationException("Could not parse card match config JSON.");
            }

            _cachedConfig = GameConfigParser.Parse(dto);
            return _cachedConfig;
        }
    }
}
