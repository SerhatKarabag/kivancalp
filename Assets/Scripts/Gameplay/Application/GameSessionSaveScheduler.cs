using System;

namespace Kivancalp.Gameplay.Application
{
    internal sealed class GameSessionSaveScheduler
    {
        private readonly float _saveDebounceSeconds;

        private bool _dirty;
        private float _saveTimer;

        public GameSessionSaveScheduler(float saveDebounceSeconds)
        {
            _saveDebounceSeconds = saveDebounceSeconds < 0f ? 0f : saveDebounceSeconds;
        }

        public void Tick(float deltaTime, Func<bool> saveOperation)
        {
            if (!_dirty)
            {
                return;
            }

            _saveTimer += deltaTime;

            if (_saveTimer < _saveDebounceSeconds)
            {
                return;
            }

            ApplyPersistResult(InvokeSave(saveOperation));
        }

        public void MarkDirty(Func<bool> saveOperation)
        {
            _dirty = true;

            if (_saveDebounceSeconds <= 0f)
            {
                ApplyPersistResult(InvokeSave(saveOperation));
            }
        }

        public void ForceSave(Func<bool> saveOperation)
        {
            ApplyPersistResult(InvokeSave(saveOperation));
        }

        public void Reset()
        {
            _dirty = false;
            _saveTimer = 0f;
        }

        private static bool InvokeSave(Func<bool> saveOperation)
        {
            if (saveOperation == null)
            {
                throw new ArgumentNullException(nameof(saveOperation));
            }

            return saveOperation();
        }

        private void ApplyPersistResult(bool saveSucceeded)
        {
            _dirty = !saveSucceeded;
            _saveTimer = 0f;
        }
    }
}
