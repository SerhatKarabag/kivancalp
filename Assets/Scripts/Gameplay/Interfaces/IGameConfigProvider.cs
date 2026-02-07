using Kivancalp.Gameplay.Configuration;

namespace Kivancalp.Gameplay.Interfaces
{
    public interface IGameConfigProvider
    {
        GameConfig Load();
    }
}
