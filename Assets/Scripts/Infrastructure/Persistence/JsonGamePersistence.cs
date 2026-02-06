using System;
using System.IO;
using Kivancalp.Core.Logging;
using Kivancalp.Gameplay.Contracts;
using UnityEngine;

namespace Kivancalp.Infrastructure.Persistence
{
    public sealed class JsonGamePersistence : IGamePersistence
    {
        private readonly IGameLogger _logger;
        private readonly string _saveFilePath;

        public JsonGamePersistence(IGameLogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _saveFilePath = Path.Combine(Application.persistentDataPath, SaveFileNames.MainSaveFile);
        }

        public bool TryLoad(out PersistedGameState persistedState)
        {
            persistedState = null;

            try
            {
                if (!File.Exists(_saveFilePath))
                {
                    return false;
                }

                string json = File.ReadAllText(_saveFilePath);

                if (string.IsNullOrWhiteSpace(json))
                {
                    return false;
                }

                persistedState = JsonUtility.FromJson<PersistedGameState>(json);
                return persistedState != null;
            }
            catch (Exception exception)
            {
                _logger.Error("Failed to load persisted game state.", exception);
                return false;
            }
        }

        public void Save(PersistedGameState persistedState)
        {
            if (persistedState == null)
            {
                throw new ArgumentNullException(nameof(persistedState));
            }

            try
            {
                string directory = Path.GetDirectoryName(_saveFilePath);

                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string json = JsonUtility.ToJson(persistedState, false);
                File.WriteAllText(_saveFilePath, json);
            }
            catch (Exception exception)
            {
                _logger.Error("Failed to save game state.", exception);
            }
        }

        public void Delete()
        {
            try
            {
                if (File.Exists(_saveFilePath))
                {
                    File.Delete(_saveFilePath);
                }
            }
            catch (Exception exception)
            {
                _logger.Error("Failed to delete persisted game state.", exception);
            }
        }
    }
}
