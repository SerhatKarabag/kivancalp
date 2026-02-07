using System;
using System.IO;
using Kivancalp.Core;
using Kivancalp.Gameplay.Interfaces;
using Kivancalp.Gameplay.Models;
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

        public bool Save(PersistedGameState persistedState)
        {
            if (persistedState == null)
            {
                throw new ArgumentNullException(nameof(persistedState));
            }

            string tempFilePath = _saveFilePath + ".tmp";
            string backupFilePath = _saveFilePath + ".bak";

            try
            {
                string directory = Path.GetDirectoryName(_saveFilePath);

                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string json = JsonUtility.ToJson(persistedState, false);
                File.WriteAllText(tempFilePath, json);

                if (File.Exists(_saveFilePath))
                {
                    if (!TryReplace(tempFilePath, _saveFilePath, backupFilePath))
                    {
                        File.Delete(_saveFilePath);
                        File.Move(tempFilePath, _saveFilePath);
                    }
                }
                else
                {
                    File.Move(tempFilePath, _saveFilePath);
                }

                return true;
            }
            catch (Exception exception)
            {
                _logger.Error("Failed to save game state.", exception);
                return false;
            }
            finally
            {
                TryDeleteIfExists(tempFilePath);
                TryDeleteIfExists(backupFilePath);
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

        private bool TryReplace(string sourceFilePath, string destinationFilePath, string backupFilePath)
        {
            try
            {
                File.Replace(sourceFilePath, destinationFilePath, backupFilePath, true);
                return true;
            }
            catch (PlatformNotSupportedException)
            {
                return false;
            }
            catch (NotSupportedException)
            {
                return false;
            }
            catch (UnauthorizedAccessException exception)
            {
                _logger.Warning("File.Replace failed due to access restriction: " + exception.Message);
                return false;
            }
            catch (IOException exception)
            {
                _logger.Warning("File.Replace failed due to I/O error: " + exception.Message);
                return false;
            }
        }

        private void TryDeleteIfExists(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception exception)
            {
                _logger.Warning("Failed to delete temporary file: " + filePath + " — " + exception.Message);
            }
        }
    }
}
