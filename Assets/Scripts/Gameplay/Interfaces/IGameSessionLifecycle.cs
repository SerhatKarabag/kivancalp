using System;
using Kivancalp.Core;

namespace Kivancalp.Gameplay.Interfaces
{
    public interface IGameSessionLifecycle : ITickable, IDisposable
    {
        void Start();

        void ForceSave();

        void ReloadFromSave();
    }
}
