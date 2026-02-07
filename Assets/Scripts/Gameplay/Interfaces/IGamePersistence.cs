using Kivancalp.Gameplay.Models;

namespace Kivancalp.Gameplay.Interfaces
{
    public interface IGamePersistence
    {
        bool TryLoad(out PersistedGameState persistedState);

        bool Save(PersistedGameState persistedState);

        void Delete();
    }
}
