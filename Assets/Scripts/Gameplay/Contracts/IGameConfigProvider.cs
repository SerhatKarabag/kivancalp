using Kivancalp.Gameplay.Configuration;

namespace Kivancalp.Gameplay.Contracts
{
    public interface IGameConfigProvider
    {
        GameConfig Load();
    }
}
