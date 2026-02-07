using System;
using Kivancalp.Core;
using Kivancalp.Gameplay.Configuration;
using Kivancalp.Gameplay.Interfaces;
using UnityEngine;

namespace Kivancalp.Infrastructure.Config
{
    public sealed class ResourcesGameConfigProvider : IGameConfigProvider
    {
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

            GameConfigAsset configAsset = GameConfigAsset.LoadOrCreateRuntimeDefault();
            GameConfigDto dto = configAsset.ToDto();

            _cachedConfig = GameConfigParser.Parse(dto);
            return _cachedConfig;
        }
    }
}
