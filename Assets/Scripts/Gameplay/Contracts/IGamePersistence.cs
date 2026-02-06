namespace Kivancalp.Gameplay.Contracts
{
    public interface IGamePersistence
    {
        bool TryLoad(out PersistedGameState persistedState);

        void Save(PersistedGameState persistedState);

        void Delete();
    }
}
