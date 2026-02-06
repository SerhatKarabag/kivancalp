using System;
using Kivancalp.Core.Logging;
using Kivancalp.Gameplay.Configuration;
using Kivancalp.Gameplay.Contracts;
using UnityEngine;

namespace Kivancalp.Infrastructure.Config
{
    public sealed class ResourcesGameConfigProvider : IGameConfigProvider
    {
        private const string LegacyJsonResourcePath = "Game/card_match_config";

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

            GameConfigDto dto = TryLoadFromAsset();

            if (dto == null)
            {
                dto = TryLoadFromLegacyJson();
            }

            if (dto == null)
            {
                _logger.Warning("Game config asset not found. Falling back to runtime defaults.");
                dto = GameConfigAsset.LoadOrCreateRuntimeDefault().ToDto();
            }

            _cachedConfig = GameConfigParser.Parse(dto);
            return _cachedConfig;
        }

        private static GameConfigDto TryLoadFromAsset()
        {
            GameConfigAsset configAsset = Resources.Load<GameConfigAsset>(GameConfigAsset.ResourcePath);

            if (configAsset == null)
            {
                return null;
            }

            configAsset.EnsureInitialized();
            return configAsset.ToDto();
        }

        private GameConfigDto TryLoadFromLegacyJson()
        {
            TextAsset legacyConfigAsset = Resources.Load<TextAsset>(LegacyJsonResourcePath);

            if (legacyConfigAsset == null)
            {
                return null;
            }

            GameConfigDto dto = JsonUtility.FromJson<GameConfigDto>(legacyConfigAsset.text);

            if (dto == null)
            {
                throw new InvalidOperationException("Could not parse legacy card match config JSON.");
            }

            _logger.Warning("Using legacy card_match_config JSON. Prefer Resources/game_config.asset.");
            return dto;
        }
    }
}
